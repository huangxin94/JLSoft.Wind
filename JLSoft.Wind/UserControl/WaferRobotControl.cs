using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace JLSoft.Wind.UserControl
{
    public class WaferRobotVisualizer : Control
    {
        // 核心状态参数
        private float _position = 0.5f;    // 水平位置 0~1
        private float _zHeight = 0.3f;    // Z轴高度 0~1
        private bool _gripping;           // 夹持状态
        private Color _statusColor = Color.LimeGreen;

        // 动画参数
        private float _currentPos;
        private float _currentZ;
        private readonly Timer _animTimer = new Timer { Interval = 20 };

        public WaferRobotVisualizer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(600, 300);

            _animTimer.Tick += (s, e) => {
                _currentPos += (_position - _currentPos) * 0.1f;
                _currentZ += (_zHeight - _currentZ) * 0.1f;
                Invalidate();
            };
            _animTimer.Start();
        }

        // 状态更新方法
        public void UpdateState(float position, float zHeight, bool isGripping)
        {
            _position = Math.Clamp(position, 0f, 1f);
            _zHeight = Math.Clamp(zHeight, 0f, 1f);
            _gripping = isGripping;
            _statusColor = isGripping ? Color.OrangeRed : Color.LimeGreen;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawEnvironment(g);
            DrawRobot(g);
            DrawStatus(g);
        }

        private void DrawEnvironment(Graphics g)
        {
            // 绘制上下设备区
            using (var brush = new SolidBrush(Color.FromArgb(30, Color.Blue)))
            {
                g.FillRectangle(brush, 0, 0, Width, 80);          // 上设备区
                g.FillRectangle(brush, 0, Height - 80, Width, 80);  // 下设备区
            }

            // 绘制轨道
            int railY = Height - 65;
            using (var pen = new Pen(Color.Silver, 4))
            {
                g.DrawLine(pen, 50, railY, Width - 50, railY);
            }
        }

        private void DrawRobot(Graphics g)
        {
            float baseX = 50 + (Width - 100) * _currentPos;
            float baseY = Height - 65;
            float zOffset = 150 * _currentZ;

            // 绘制Z轴立柱
            using (var zPen = new Pen(Color.Gray, 8))
            {
                g.DrawLine(zPen, baseX, baseY, baseX, baseY - zOffset);
            }

            // 绘制机械臂
            DrawArm(g, baseX, baseY - zOffset);
            DrawEndEffector(g, baseX, baseY - zOffset);
        }

        private void DrawArm(Graphics g, float x, float y)
        {
            // 主臂
            using (var armPen = new Pen(Color.DimGray, 12))
            {
                g.DrawLine(armPen, x, y, x + 80, y - 100);
            }

            // 关节
            using (var jointBrush = new SolidBrush(Color.Gold))
            {
                g.FillEllipse(jointBrush, x - 12, y - 12, 24, 24);
            }
        }

        private void DrawEndEffector(Graphics g, float x, float y)
        {
            // 夹爪状态
            int gripWidth = _gripping ? 20 : 40;
            using (var gripPen = new Pen(_statusColor, 6))
            {
                g.DrawLine(gripPen, x + 80, y - 100, x + 80 + gripWidth, y - 130);
                g.DrawLine(gripPen, x + 80, y - 100, x + 80 - gripWidth, y - 130);
            }

            // 晶圆示意
            if (_gripping)
            {
                using (var waferBrush = new SolidBrush(Color.FromArgb(100, Color.Purple)))
                {
                    g.FillEllipse(waferBrush, x + 30, y - 150, 100, 20);
                }
            }
        }

        private void DrawStatus(Graphics g)
        {
            string status = $"Position: {_currentPos:P0}\nZ Height: {_currentZ:P0}";
            using (var font = new Font("Arial", 10))
            using (var brush = new SolidBrush(_statusColor))
            {
                g.DrawString(status, font, brush, 10, 10);
            }
        }
    }
}
