using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Diagnostics;
using Sunny.UI;
using JLSoft.Wind.CustomControl;
using JLSoft.Wind.Adapter;

namespace JLSoft.Wind.UserControl
{
    public partial class FactoryLayoutControl : System.Windows.Forms.UserControl
    {
        #region 字段和常量
        private readonly Dictionary<Control, Rectangle> _originalBounds = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, float> _originalFontSizes = new Dictionary<Control, float>(); // 新增：存储原始字体大小
        private bool _isScaling;
        private float _lastScaleX = 1f, _lastScaleY = 1f;
        private const float _minScale = 0.3f;
        private const float _maxScale = 3.0f;
        private const float ScaleThreshold = 0.01f;
        private const float FontScaleThreshold = 0.1f;
        private const float MinFontSize = 8f;
        private const float MaxFontSize = 30f; // 修改：字体最大不超过30

        private DateTime _lastRenderTime = DateTime.MinValue;
        private const int RenderInterval = 50;
        private Bitmap _cachedRotatedImage;
        private float _rotationAngle;
        private Point _dragStartPosition;

        private readonly Size _designTimeSize = new Size(988, 256);
        private readonly System.Windows.Forms.Timer _resizeTimer = new() { Interval = 300 };

        public event EventHandler<ControlOperationEventArgs> ControlClick;
        public event EventHandler<ControlOperationEventArgs> ControlColorChanged;



        #endregion

        #region 初始化
        public FactoryLayoutControl()
        {

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
             ControlStyles.AllPaintingInWmPaint |
             ControlStyles.UserPaint |
             ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                InitializeComponent();
                AutoScaleMode = AutoScaleMode.Dpi;
                return;
            }

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw, true);

            InitializeControlEvents();
            this.Load += FactoryLayoutControl_Load;
            this.SizeChanged += FactoryLayoutControl_SizeChanged;

            _resizeTimer.Tick += ResizeTimer_Tick;
        }

        private void FactoryLayoutControl_Load(object sender, EventArgs e)
        {
            InitializeOriginalBounds();
        }

        private void InitializeControlEvents()
        {
            // 订阅DeviceBlock点击事件
            var deviceBlocks = new[] {
                deviceBlock1, deviceBlock2, deviceBlock3, deviceBlock4,
                deviceBlock5, deviceBlock6, deviceBlock7, deviceBlock8,
                deviceBlock9, deviceBlock10, deviceBlock11, deviceBlock12
            };
            foreach (var block in deviceBlocks)
            {
                if (block != null) block.Click += DeviceBlock_Click;
            }
            deviceBlock21.Click += DeviceBlock_Click;
            deviceBlock22.Click += DeviceBlock_Click;
            pseudoCubeControl1.Click += PseudoCube_Click;
        }
        #endregion

        #region 原始边界初始化（修复字体缩放问题）
        private void InitializeOriginalBounds()
        {
            foreach (Control control in GetAllControls(this))
            {
                // 保存位置和大小
                _originalBounds[control] = new Rectangle(
                    control.Location.X,
                    control.Location.Y,
                    control.Size.Width,
                    control.Size.Height);

                // 保存原始字体大小 - 修复字体缩放问题
                if (control.Font != null)
                {
                    _originalFontSizes[control] = control.Font.Size;
                }
            }
        }
        #endregion

        #region 拉伸铺满的缩放实现（修复字体缩放问题）
        private void FactoryLayoutControl_SizeChanged(object sender, EventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;

            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            DoStretchScaling();
        }
        public void ForceRescale()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            this.BeginInvoke((Action)(() =>
            {
                DoStretchScaling();
            }));
        }
        public void DoStretchScaling()
        {
            if (this.Disposing || this.IsDisposed || !this.Visible)
                return;
            if (_isScaling || Width <= 0 || Height <= 0)
                return;

            float scaleX = (float)Width / _designTimeSize.Width;
            float scaleY = (float)Height / _designTimeSize.Height;

            if (Math.Abs(scaleX - _lastScaleX) < 0.01f &&
                Math.Abs(scaleY - _lastScaleY) < 0.01f)
                return;

            scaleX = Math.Clamp(scaleX, _minScale, _maxScale);
            scaleY = Math.Clamp(scaleY, _minScale, _maxScale);

            /*
            if (Math.Abs(scaleX - _lastScaleX) < ScaleThreshold &&
                Math.Abs(scaleY - _lastScaleY) < ScaleThreshold)
                return;
            */

            _isScaling = true;
            try
            {
                using (new SuspendDrawingScope(this)) // 使用暂停绘制的辅助类
                {
                    ApplyStretchScaling(scaleX, scaleY);
                }
            }
            finally
            {
                _isScaling = false;
                _lastScaleX = scaleX;
                _lastScaleY = scaleY;
            }
        }

        private void ApplyStretchScaling(float scaleX, float scaleY)
        {
            try
            {
                SuspendLayout();
                float avgScale = (scaleX + scaleY) / 2;

                foreach (var kv in _originalBounds)
                {
                    var ctrl = kv.Key;
                    var orig = kv.Value;

                    // 应用位置和大小缩放
                    ctrl.Location = new Point(
                        (int)(orig.X * scaleX),
                        (int)(orig.Y * scaleY));

                    ctrl.Size = new Size(
                        (int)(orig.Width * scaleX),
                        (int)(orig.Height * scaleY));

                    // 更新字体大小（基于原始字体大小）
                    UpdateFontScale(ctrl, avgScale);

                    // 特殊处理：更新PseudoCubeControl和DeviceBlock2的内部字体
                    UpdateSpecialControlsFont(ctrl, avgScale);
                }
            }
            finally
            {
                ResumeLayout(true);
            }
        }
        private void UpdateSpecialControlsFont(Control control, float scale)
        {
            // 处理PseudoCubeControl
            if (control is PseudoCubeControl pseudoCube)
            {
                // 计算控件可容纳的最大字体大小
                float maxHeight = pseudoCube.Height * 0.8f; // 保留20%边距
                float maxWidth = pseudoCube.Width * 0.8f;

                if (pseudoCube.LabelFont != null)
                {
                    float targetSize = pseudoCube.LabelFont.Size * scale;

                    // 限制字体不超过控件尺寸
                    using (Graphics g = pseudoCube.CreateGraphics())
                    {
                        var textSize = g.MeasureString(pseudoCube.LabelText,
                            new Font(pseudoCube.LabelFont.FontFamily, targetSize));

                        while (textSize.Height > maxHeight || textSize.Width > maxWidth)
                        {
                            targetSize -= 0.5f;
                            if (targetSize <= MinFontSize) break;
                            textSize = g.MeasureString(pseudoCube.LabelText,
                                new Font(pseudoCube.LabelFont.FontFamily, targetSize));
                        }
                    }

                    targetSize = Math.Clamp(targetSize, MinFontSize, MaxFontSize);

                    if (Math.Abs(pseudoCube.LabelFont.Size - targetSize) > FontScaleThreshold)
                    {
                        pseudoCube.LabelFont = new Font(
                            pseudoCube.LabelFont.FontFamily,
                            targetSize,
                            pseudoCube.LabelFont.Style);
                        pseudoCube.Invalidate(); // 强制重绘
                    }
                }
            }
            // 处理DeviceBlock2
            if (control is DeviceBlock2 deviceBlock2)
            {
                // 更新设备代码字体
                if (deviceBlock2.DeviceCodeFont != null)
                {
                    float targetSize = deviceBlock2.DeviceCodeFont.Size * scale;

                    // 限制字体不超过控件尺寸
                    using (Graphics g = deviceBlock2.CreateGraphics())
                    {
                        var textSize = g.MeasureString(deviceBlock2.DeviceCode,
                            new Font(deviceBlock2.DeviceCodeFont.FontFamily, targetSize));

                        while (textSize.Width > deviceBlock2.Width * 0.8f ||
                               textSize.Height > deviceBlock2.Height * 0.8f)
                        {
                            targetSize -= 0.5f;
                            if (targetSize <= MinFontSize) break;
                            textSize = g.MeasureString(deviceBlock2.DeviceCode,
                                new Font(deviceBlock2.DeviceCodeFont.FontFamily, targetSize));
                        }
                    }

                    targetSize = Math.Clamp(targetSize, MinFontSize, MaxFontSize);

                    if (Math.Abs(deviceBlock2.DeviceCodeFont.Size - targetSize) > FontScaleThreshold)
                    {
                        deviceBlock2.DeviceCodeFont = new Font(
                            deviceBlock2.DeviceCodeFont.FontFamily,
                            targetSize,
                            deviceBlock2.DeviceCodeFont.Style);
                    }
                }

                // 更新状态值字体
                if (deviceBlock2.StatusValueFont != null)
                {
                    float targetSize = deviceBlock2.StatusValueFont.Size * scale;

                    // 限制字体不超过控件尺寸
                    using (Graphics g = deviceBlock2.CreateGraphics())
                    {
                        var textSize = g.MeasureString(deviceBlock2.StatusValue,
                            new Font(deviceBlock2.StatusValueFont.FontFamily, targetSize));

                        while (textSize.Width > deviceBlock2.Width * 0.9f ||
                               textSize.Height > deviceBlock2.Height * 0.9f)
                        {
                            targetSize -= 0.5f;
                            if (targetSize <= MinFontSize) break;
                            textSize = g.MeasureString(deviceBlock2.StatusValue,
                                new Font(deviceBlock2.StatusValueFont.FontFamily, targetSize));
                        }
                    }

                    targetSize = Math.Clamp(targetSize, MinFontSize, MaxFontSize);

                    if (Math.Abs(deviceBlock2.StatusValueFont.Size - targetSize) > FontScaleThreshold)
                    {
                        deviceBlock2.StatusValueFont = new Font(
                            deviceBlock2.StatusValueFont.FontFamily,
                            targetSize,
                            deviceBlock2.StatusValueFont.Style);
                    }
                }
                deviceBlock2.Invalidate(); // 强制重绘
            }
        }
        private void UpdateFontScale(Control control, float scale)
        {
            // 从原始字体大小开始缩放 - 修复字体缩放问题
            if (_originalFontSizes.TryGetValue(control, out float originalSize))
            {
                float targetSize = originalSize * scale;
                targetSize = Math.Clamp(targetSize, MinFontSize, MaxFontSize);

                if (control.Font != null &&
                    Math.Abs(control.Font.Size - targetSize) > FontScaleThreshold)
                {
                    control.Font = new Font(control.Font.FontFamily, targetSize, control.Font.Style);
                }
            }
        }


        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && this.Parent != null)
            {
                ForceRescale();
            }
        }
        #endregion

        #region 旋转与拖动
        public void RotateImage(float degrees)
        {
            const float angleThreshold = 2f;
            if (Math.Abs(degrees) < angleThreshold) return;
            if ((DateTime.Now - _lastRenderTime).TotalMilliseconds < RenderInterval) return;

            _rotationAngle += degrees;
            _rotationAngle %= 360;

            if (pictureBox1.Image == null) return;

            Task.Run(() =>
            {
                using var original = new Bitmap(pictureBox1.Image);
                Bitmap rotated = RotateImageInternal(original, _rotationAngle);
                return rotated;
            }).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && !IsDisposed)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (_cachedRotatedImage != null)
                        {
                            _cachedRotatedImage.Dispose();
                        }
                        _cachedRotatedImage = t.Result;
                        pictureBox1.Image = _cachedRotatedImage;
                        _lastRenderTime = DateTime.Now;
                    });
                }
            });
        }

        private Bitmap RotateImageInternal(Image original, float angle)
        {
            var bmp = new Bitmap(original.Width, original.Height);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TranslateTransform(original.Width / 2f, original.Height / 2f);
            g.RotateTransform(angle);
            g.TranslateTransform(-original.Width / 2f, -original.Height / 2f);
            g.DrawImage(original, 0, 0);
            return bmp;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _dragStartPosition = e.Location;
            Cursor = Cursors.SizeAll;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int newX = pictureBox1.Left + (e.X - _dragStartPosition.X);
            newX = Math.Clamp(newX, 0, uiPanel2.Width - pictureBox1.Width);
            pictureBox1.Left = newX;
        }
        #endregion

        #region 事件处理
        private void DeviceBlock_Click(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                ControlClick?.Invoke(this, new ControlOperationEventArgs(control));
            }
        }

        private void PseudoCube_Click(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                ControlClick?.Invoke(this, new ControlOperationEventArgs(control));
            }
        }
        #endregion

        #region 辅助方法
        private IEnumerable<Control> GetAllControls(Control parent)
        {
            var stack = new Stack<Control>(Controls.Count);
            foreach (Control c in Controls) stack.Push(c);

            while (stack.Count > 0)
            {
                var c = stack.Pop();
                yield return c;

                foreach (Control child in c.Controls)
                    stack.Push(child);
            }
        }

        public void SetControlsBackColor(Color color, bool excludeLabels = true)
        {
            try
            {
                foreach (Control c in GetAllControls(this))
                {
                    if (excludeLabels && c is Label) continue;
                    c.BackColor = color;
                }
                ControlColorChanged?.Invoke(this, new ControlOperationEventArgs(null));
            }
            catch (Exception ex)
            {
                ControlColorChanged?.Invoke(this, new ControlOperationEventArgs(null, ex.Message));
            }
        }

        public void SetDeviceBlockColor(string deviceCode, Color color)
        {
            foreach (Control ctrl in GetAllControls(this))
            {
                // 假设你的设备块控件类型为 DeviceBlock 或 DeviceBlock2，并有 DeviceCode 属性
                if (ctrl is DeviceBlock block && block.DeviceCode == deviceCode)
                {
                    block.StatusColor = color;
                    return;
                }
                if (ctrl is DeviceBlock2 block2 && block2.DeviceCode == deviceCode)
                {
                    block2.BackColor = color;
                    return;
                }
            }
        }
        public void SetDeviceBlockBorderColor(string deviceCode, DeviceStatus device)
        {
            foreach (Control ctrl in GetAllControls(this))
            {
                // 假设你的设备块控件类型为 DeviceBlock 或 DeviceBlock2，并有 DeviceCode 属性
                if (ctrl is DeviceBlock block && block.DeviceCode == deviceCode)
                {
                    block.DeviceStatus = device;
                    return;
                }
            }
        }
        #endregion

        #region 辅助类
        public class ControlOperationEventArgs : EventArgs
        {
            public Control TargetControl { get; }
            public string ErrorMessage { get; }
            public ControlOperationEventArgs(Control control, string error = null)
            {
                TargetControl = control;
                ErrorMessage = error;
            }
        }
        #endregion
    }
}