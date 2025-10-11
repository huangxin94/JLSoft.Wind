using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace JLSoft.Wind.UIHelpers
{
    public partial class LoadingOverlay : Form
    {
        private readonly CircularProgressBar _progressBar;
        private bool _allowClose = false;
        private Label _label;

        public LoadingOverlay(Form parent) : this(parent, "处理中...")
        {
        }
        public LoadingOverlay(Form parent, string loadingText)
        {
            // 基础窗体设置
            StartPosition = FormStartPosition.CenterParent;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            //StartPosition = FormStartPosition.Manual; 
            BackColor = Color.Black; // 纯黑色背景
            Opacity = 0.9; // 90% 透明度
            Size = new Size(220, 160);


            Owner = parent; // 设置父窗体
            TopMost = true; // 强制置顶

            // 居中显示
            Location = new Point(
                parent.Location.X + (parent.Width - Width) / 2,
                parent.Location.Y + (parent.Height - Height) / 2
            );

            // 启用双缓冲
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            // 创建进度条
            _progressBar = new CircularProgressBar
            {
                Size = new Size(80, 80),
                Location = new Point(70, 20),
            };
            Controls.Add(_progressBar);

            // 添加文字
            var label = new Label
            {
                Text = loadingText,
                Font = new Font("幼圆", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(180, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 110),
                BackColor = Color.Transparent
            };
            Controls.Add(label);

            // 添加圆角效果
            Region = CreateRoundRegion(ClientRectangle, 15);

            var autoCloseTimer = new Timer { Interval = 15000 }; // 15秒后自动关闭
            autoCloseTimer.Tick += (s, e) => {
                if (!this.IsDisposed) this.Close();
            };
            autoCloseTimer.Start();
        }
        public void SafeClose()
        {
            _allowClose = true;
            if (!this.IsDisposed && this.IsHandleCreated)
                this.BeginInvoke((Action)(() => this.Close()));
        }
        public void SetLoadingText(string text)
        {
            if (_label != null && !_label.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(SetLoadingText), text);
                }
                else
                {
                    _label.Text = text;
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 只允许通过SafeClose关闭
            if (!_allowClose)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }
        // 创建圆角区域
        private Region CreateRoundRegion(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        // 自定义圆形进度条（解决透明问题）
        private class CircularProgressBar : Control
        {
            private int _angle;
            private readonly Timer _timer;

            public CircularProgressBar()
            {
                // 启用双缓冲和透明背景支持
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);

                BackColor = Color.Transparent;
                DoubleBuffered = true;

                _timer = new Timer { Interval = 30 };
                _timer.Tick += (s, e) => {
                    _angle = (_angle + 10) % 360;
                    Invalidate();
                };
                _timer.Start();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 计算中心点
                int centerX = Width / 2;
                int centerY = Height / 2;
                int radius = Math.Min(Width, Height) / 2 - 10;

                // 绘制背景圆环
                using (var bgPen = new Pen(Color.FromArgb(220, 220, 220), 6))
                {
                    g.DrawEllipse(bgPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }

                // 绘制进度弧
                using (var progressPen = new Pen(Color.SteelBlue, 6))
                {
                    g.DrawArc(progressPen, centerX - radius, centerY - radius, radius * 2, radius * 2,
                             _angle, 90); // 90度弧长
                }
            }

            protected override void Dispose(bool disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                base.Dispose(disposing);
            }
        }

        // 重写背景绘制实现透明效果
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            // 绘制半透明背景
            using (var brush = new SolidBrush(Color.FromArgb(180, Color.LightGray)))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
