using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace JLSoft.Wind.UserControl
{
    // 设备状态枚举
    public enum DeviceStatus
    {
        Running,     // 正在生产
        Offline,     // 离线
        Fault,       // 异常
        Idle,        // 空闲
        Paused       // 暂停
    }

    [DesignerCategory("Code")]
    public class DeviceBlock : System.Windows.Forms.UserControl
    {
        // 保留原有小圆点颜色字段
        private Color _statusColor = Color.Green;
        private string _deviceCode = "W 0001C80E";
        private bool _isDragging;
        private Point _dragStart;

        // 新增设备状态字段
        private DeviceStatus _deviceStatus = DeviceStatus.Idle;

        public DeviceBlock()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint,
                    true);
            DoubleBuffered = true;
            // 设置默认尺寸
            Size = new Size(150, 80);
        }

        [Browsable(true)]
        [Description("设备整体状态（控制边框颜色）")]
        public DeviceStatus DeviceStatus
        {
            get => _deviceStatus;
            set
            {
                if (_deviceStatus != value)
                {
                    _deviceStatus = value;
                    Invalidate(); // 重绘边框
                }
            }
        }

        [Browsable(true)]
        [Description("设备内部小圆点颜色（表示设备内是否有产品）")]
        public Color StatusColor
        {
            get => _statusColor;
            set
            {
                if (_statusColor != value)
                {
                    _statusColor = value;
                    Invalidate();
                }
            }
        }

        [Browsable(true)]
        [Description("设备编码")]
        public string DeviceCode
        {
            get => _deviceCode;
            set { _deviceCode = value; Invalidate(); }
        }

        [Browsable(true)]
        [Description("是否允许拖拽")]
        public bool IsDraggable { get; set; } = true;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // 根据设备状态设置边框颜色
            Color borderColor = _deviceStatus switch
            {
                DeviceStatus.Offline => Color.Gray,
                DeviceStatus.Fault => Color.Red,
                DeviceStatus.Idle => Color.Green,
                DeviceStatus.Running => Color.Blue,
                DeviceStatus.Paused => Color.Orange,
                _ => Color.Black
            };

            // 绘制状态边框（加粗到4px）
            using (var borderPen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(borderPen, 2, 2, Width - 5, Height - 5);
            }

            // 保留原有3D效果边框
            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // 绘制设备代码（保留抗锯齿）
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var textBrush = new SolidBrush(ForeColor))
            {
                g.DrawString(_deviceCode, Font, textBrush, new PointF(5, 5));
            }

            // 保留原有小圆点设计（右下角）
            using (var brush = new SolidBrush(_statusColor))
            {
                int indicatorSize = 15;
                int x = Width - indicatorSize - 5;
                int y = Height - indicatorSize - 5;
                g.FillEllipse(brush, x, y, indicatorSize, indicatorSize);
            }
        }

        // 保留原有拖拽功能
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsDraggable)
            {
                _isDragging = true;
                _dragStart = e.Location;
                Cursor = Cursors.SizeAll;
                Capture = true;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging && IsDraggable)
            {
                Location = new Point(
                    Location.X + (e.X - _dragStart.X),
                    Location.Y + (e.Y - _dragStart.Y));
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsDraggable)
            {
                _isDragging = false;
                Cursor = Cursors.Default;
                Capture = false;
            }
            base.OnMouseUp(e);
        }
    }
}