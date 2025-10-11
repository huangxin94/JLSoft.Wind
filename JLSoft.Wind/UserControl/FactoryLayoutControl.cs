using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Adapter;
using JLSoft.Wind.CustomControl;
using JLSoft.Wind.Services.Connect;
using Sunny.UI;
using static JLSoft.Wind.Services.Status.DeviceMonitor;

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


        #region 机器人移动相关字段
        private System.Windows.Forms.Timer _animationTimer;
        private Point _robotTargetPosition;
        private float _robotTargetAngle;
        private const int AnimationInterval = 20; // 20ms动画帧
        private const float AnimationSpeed = 0.1f; // 动画速度系数
        private string _targetDeviceCode;

        // 添加设备方向字典
        private readonly Dictionary<string, Direction> _deviceDirections = new Dictionary<string, Direction>();

        // 添加机器人状态枚举
        private enum RobotState { Idle, Moving, Rotating, MovingAfterRotation }
        private RobotState _robotState = RobotState.Idle;

        // 添加方向枚举（修复访问级别问题）
        public enum Direction { Top, Right, Bottom, Left }

        private System.Windows.Forms.Timer _rotationTimer = new System.Windows.Forms.Timer() { Interval = 20 };
        private float _rotationStep = 0f;
        private const float RotationSpeed = 15f; // 每次旋转的度数
        private float _targetRotation = 0f;
        private enum RotationDirection { Clockwise, CounterClockwise }
        #endregion

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


            _animationTimer = new System.Windows.Forms.Timer { Interval = AnimationInterval };
            _animationTimer.Tick += AnimationTimer_Tick;

            // 初始化设备信息
            InitializeDeviceInfo(); 
            InitializeRotationTimer();
            _rotationTimer.Interval = 20;


        }

        #region 新增Robot移动与设备定位功能



        private void InitializeRotationTimer()
        {
            _rotationTimer.Tick += (s, e) =>
            {
                if (_robotState != RobotState.Rotating) return;
                // 计算最短旋转路径
                float delta = _targetRotation - _rotationAngle;
                float absDelta = Math.Abs(delta);

                // 处理跨360°的旋转
                if (absDelta > 180)
                {
                    delta = delta < 0 ? delta + 360 : delta - 360;
                    absDelta = Math.Abs(delta);
                }

                if (absDelta < RotationSpeed)
                {
                    _rotationAngle = _targetRotation;
                    _rotationTimer.Stop();
                    ApplyRotation(_rotationAngle);
                    UpdatePositionIndicator(_targetDeviceCode);
                    _robotState = RobotState.Idle;// 完成后设为空闲
                    return;
                }

                // 确定旋转方向
                //RotationDirection direction = delta > 0 ?
                //    RotationDirection.Clockwise :
                //    RotationDirection.CounterClockwise;

                // 应用旋转
                float rotationStep = Math.Sign(delta) * Math.Min(RotationSpeed, absDelta);
                RotateImage(rotationStep);

                //RotateImage(rotationStep);
                //ApplyRotation(_rotationAngle);
            };
        }

        // 添加机器人状态枚举
        private void InitializeDeviceInfo()
        {
            // 添加设备信息：位置和默认指向方向
            _deviceDirections.Add("P1", Direction.Left);
            _deviceDirections.Add("S4", Direction.Top);
            _deviceDirections.Add("V1", Direction.Bottom);
            _deviceDirections.Add("M1", Direction.Top);
            _deviceDirections.Add("S3", Direction.Top);
            _deviceDirections.Add("U1", Direction.Bottom);
            _deviceDirections.Add("C1", Direction.Top);
            _deviceDirections.Add("G1", Direction.Bottom);
            _deviceDirections.Add("R1", Direction.Top);
            _deviceDirections.Add("寻边器", Direction.Bottom);
            _deviceDirections.Add("角度台", Direction.Bottom);
            _deviceDirections.Add("A1", Direction.Top);
            _deviceDirections.Add("A2", Direction.Top);
            _deviceDirections.Add("A3", Direction.Top);
            _deviceDirections.Add("A4", Direction.Top);
            _deviceDirections.Add("LoadPort1", Direction.Bottom);
            _deviceDirections.Add("LoadPort2", Direction.Bottom);
        }
        private Direction _targetDirection; // 目标方向
        private Label _debugLabel;
        public void SmoothMoveRobotToDevice(string deviceCode)
        {
            if (!_deviceDirections.TryGetValue(deviceCode, out var direction))
                return;

            _targetDirection = direction; // 存储目标方向

            var deviceControl = FindDeviceControl(deviceCode);
            if (deviceControl == null) return;

            // 停止所有动画
            _animationTimer.Stop();
            _rotationTimer.Stop();

            // 计算目标位置和角度
            Point screenPoint = deviceControl.PointToScreen(Point.Empty);
            Point panelPoint = uiPanel2.PointToClient(screenPoint);
            _robotTargetPosition = new Point(
                panelPoint.X + deviceControl.Width / 2 - pictureBox1.Width / 2,
                pictureBox1.Top
            );
            _targetRotation = GetRotationAngle(direction);
            _targetDeviceCode = deviceCode;
            float rotationNeeded = GetRelativeRotation(direction);

            // 优先处理旋转
            _robotState = RobotState.Moving;
            _animationTimer.Start();

            if (_rotationTimer.Enabled)
            {
                _rotationTimer.Stop();
            }
        }


        private float GetRelativeRotation(Direction targetDirection)
        {
            // 假设机器人当前方向存储在_currentDirection
            var current = _currentDirection;
            var target = targetDirection;

            // 计算最短旋转路径
            if (current == target) return 0f;

            // 计算顺时针和逆时针的旋转角度
            int clockwise = (4 + (int)target - (int)current) % 4 * 90;
            int counterClockwise = clockwise - 360;

            // 返回绝对值最小的旋转角度
            return Math.Abs(clockwise) < Math.Abs(counterClockwise)
                ? clockwise : counterClockwise;
        }
        // 优化后的动画帧处理
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            switch (_robotState)
            {
                case RobotState.Moving:
                    HandleMovement();
                    break;
                case RobotState.Rotating:
                    HandleRotation();
                    break;
            }
        }
        private float _robotCurrentPositionX;
        private void HandleMovement()
        {

            float targetX = _robotTargetPosition.X;
            float dx = targetX - _robotCurrentPositionX;
            float absDx = Math.Abs(dx);

            int maxX = uiPanel2.Width - pictureBox1.Width;

            if (targetX < 0) targetX = 0;
            if (targetX > maxX) targetX = maxX;

            // 重新计算距离
            dx = targetX - _robotCurrentPositionX;
            absDx = Math.Abs(dx);


            if ((_robotCurrentPositionX <= 0 && dx < 0) ||
                (_robotCurrentPositionX >= maxX && dx > 0))
            {
                // 到达边界，直接开始旋转
                _robotState = RobotState.Rotating;
                return;
            }
            // 到达判定（5像素容差）
            if (absDx < 5f)
            {
                // 精确到达目标位置
                _robotCurrentPositionX = targetX;
                pictureBox1.Left = (int)Math.Round(_robotCurrentPositionX);


                // 旋转判断逻辑
                if (_currentDirection != _targetDirection)
                {
                    _robotState = RobotState.Rotating;
                }
                else
                {
                    _robotState = RobotState.Idle;
                    UpdatePositionIndicator(_targetDeviceCode);
                }
                return;
            }

            // 动态步长计算
            float step = Math.Sign(dx) * Math.Max(1f, absDx * AnimationSpeed);
            
            // 更新浮点位置
            _robotCurrentPositionX += step;

            // 边界检查
            _robotCurrentPositionX = Math.Max(0, Math.Min(maxX, _robotCurrentPositionX));

            // 更新UI位置（四舍五入）
            int newPosition = (int)Math.Round(_robotCurrentPositionX);
            pictureBox1.Left = newPosition;
        }
        private void HandleRotation()
        {
            // 计算当前剩余角度（动态计算，非固定值）

            float remaining = CalculateRemainingRotation();
            //float progress = Math.Abs(remaining) / 180f; // 旋转进度(0-1)
            //float curvedSpeed = RotationSpeed * (1 + 2 * (float)Math.Sin(progress * Math.PI)); // 正弦曲线加速


            float absRemaining = Math.Abs(remaining);


            float dynamicSpeed = Math.Min(90f, Math.Max(RotationSpeed, absRemaining * 0.5f));
            // 完成条件：小于旋转步长
            if (absRemaining <= dynamicSpeed)
            {
                // 应用最终旋转调整
                RotateImage(remaining);
                _rotationAngle = NormalizeAngle(_rotationAngle + remaining);
                _currentDirection = _targetDirection; // 更新方向

                _robotState = RobotState.Idle;
                UpdatePositionIndicator(_targetDeviceCode);
                return;
            }
            float t = Math.Abs(remaining) / 180f;
            float easeFactor = 1 - (1 - t) * (1 - t);
            float step = Math.Sign(remaining) * dynamicSpeed * easeFactor;
            // 计算旋转步长（带方向）
            //float step = Math.Sign(remaining) * dynamicSpeed;

            // 应用旋转
            RotateImage(step);
            _rotationAngle = NormalizeAngle(_rotationAngle + step);

            // 更新当前方向
            _currentDirection = GetCurrentDirection();
        }
        private float CalculateRemainingRotation()
        {
            // 计算最短路径
            float delta = _targetRotation - _rotationAngle;

            // 处理跨越360°的情况
            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;

            return delta;
        }
        private Direction GetCurrentDirection()
        {
            // 将角度标准化到0-360
            float normalized = NormalizeAngle(_rotationAngle);

            if (normalized >= 315 || normalized < 45) return Direction.Top;
            if (normalized >= 45 && normalized < 135) return Direction.Right;
            if (normalized >= 135 && normalized < 225) return Direction.Bottom;
            return Direction.Left;
        }

        #endregion
        private Control FindDeviceControl(string deviceCode)
        {
            return FindControlRecursive(this, deviceCode);
        }
        private Control FindControlRecursive(Control parent, string deviceCode)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is DeviceBlock db && db.DeviceCode == deviceCode)
                    return c;
                if (c is DeviceBlock2 db2 && db2.DeviceCode == deviceCode)
                    return c;
                if (c is PseudoCubeControl pc && pc.LabelText == deviceCode)
                    return c;

                // 递归搜索子容器
                var found = FindControlRecursive(c, deviceCode);
                if (found != null) return found;
            }
            return null;
        }
        // 获取旋转角度
        private float GetRotationAngle(Direction direction)
        {
            return direction switch
            {
                Direction.Top => 0f,
                Direction.Right => 90f,
                Direction.Bottom => 180f, // ✅ 处理下方
                Direction.Left => 270f,
                _ => 0f
            };
        }

        private Bitmap _originalRobotImage;
        #region 旋转与拖动
        // 优化旋转方法
        public void RotateImage(float degrees)
        {
            if (pictureBox1.Image == null) return;

            try
            {
                // 只创建一次原始图像的副本
                if (_originalRobotImage == null)
                {
                    _originalRobotImage = new Bitmap(pictureBox1.Image);
                }

                // 更新当前角度
                _rotationAngle = NormalizeAngle(_rotationAngle + degrees);

                // 创建旋转后的图像
                using (var rotated = RotateImageInternal(_originalRobotImage, _rotationAngle))
                {
                    if (pictureBox1.InvokeRequired)
                    {
                        pictureBox1.Invoke(() => {
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = new Bitmap(rotated); // 创建新副本
                            pictureBox1.Invalidate(); // 强制重绘
                        });
                    }
                    else
                    {
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = new Bitmap(rotated); // 创建新副本
                        pictureBox1.Invalidate(); // 强制重绘
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"旋转错误: {ex.Message}");
            }
        }
        private float NormalizeAngle(float angle)
        {
            while (angle >= 360) angle -= 360;
            while (angle < 0) angle += 360;
            return angle;
        }
        private void ApplyRotation(float angle)
        {
            if (pictureBox1.Image == null) return;

            // 创建旋转后的图像
            using (var rotated = RotateImageInternal(_originalRobotImage, angle))
            {
                if (pictureBox1.InvokeRequired)
                {
                    pictureBox1.Invoke(() => {
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = new Bitmap(rotated);
                        pictureBox1.Invalidate(); // 强制重绘
                    });
                }
                else
                {
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = new Bitmap(rotated);
                    pictureBox1.Invalidate(); // 强制重绘
                }
            }
        }
        // 优化旋转计算
        private Bitmap RotateImageInternal(Image original, float angle)
        {
            // 创建新位图
            var bmp = new Bitmap(original.Width, original.Height);

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 计算旋转中心
                float centerX = original.Width / 2f;
                float centerY = original.Height / 2f;

                // 应用变换
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angle);
                g.TranslateTransform(-centerX, -centerY);

                // 绘制图像
                g.DrawImage(original, 0, 0);
            }

            return bmp;
        }
        #endregion

        #region 位置指示器更新
        private void UpdatePositionIndicator(string deviceCode)
        {
            ClearPositionIndicators();

            if (!this.Controls.ContainsKey(deviceCode))
                return;

            var device = this.Controls[deviceCode];
            var direction = _deviceDirections.ContainsKey(deviceCode)
                ? _deviceDirections[deviceCode]
                : Direction.Right;

            // 根据设备方向设置位置
            var indicator = new Label
            {
                Text = GetDirectionSymbol(direction),
                ForeColor = Color.Red,
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Name = "PositionIndicator"
            };

            Point location = CalculateIndicatorPosition(device, direction);
            indicator.Location = location;

            this.Controls.Add(indicator);
            indicator.BringToFront();
        }
        private void ClearPositionIndicators()
        {
            var indicators = this.Controls
                .OfType<Control>()
                .Where(c => c.Name == "PositionIndicator")
                .ToList();

            foreach (var indicator in indicators)
            {
                this.Controls.Remove(indicator);
                indicator.Dispose();
            }
        }

        private string GetDirectionSymbol(Direction direction)
        {
            return direction switch
            {
                Direction.Top => "▲",
                Direction.Right => "▶",
                Direction.Bottom => "▼",
                Direction.Left => "◀",
                _ => "●"
            };
        }

        private Point CalculateIndicatorPosition(Control device, Direction direction)
        {
            int padding = 5;

            return direction switch
            {
                Direction.Top => new Point(
                    device.Location.X + device.Width / 2 - 8,
                    device.Location.Y - padding - 20),

                Direction.Right => new Point(
                    device.Location.X + device.Width + padding,
                    device.Location.Y + device.Height / 2 - 10),

                Direction.Bottom => new Point(
                    device.Location.X + device.Width / 2 - 8,
                    device.Location.Y + device.Height + padding),

                Direction.Left => new Point(
                    device.Location.X - padding - 20,
                    device.Location.Y + device.Height / 2 - 10),

                _ => new Point(
                    device.Location.X + device.Width / 2 - 8,
                    device.Location.Y - padding - 20)
            };
        }
        #endregion

        Direction _currentDirection = Direction.Top; // 机器人当前方向，默认向上
        private void FactoryLayoutControl_Load(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null && _originalRobotImage == null)
            {
                _originalRobotImage = new Bitmap(pictureBox1.Image);
                // 初始方向向上
                _currentDirection = Direction.Top;
            }

            _robotCurrentPositionX = pictureBox1.Left;
            _currentDirection = Direction.Top;
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