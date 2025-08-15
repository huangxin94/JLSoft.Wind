using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.UserControl
{
    // 轨道和机械臂 TrackControl.cs
    public class TrackControl : Panel
    {
        private PointF robotPosition = new PointF(100, 150);
        private PointF targetPosition = new PointF(400, 150);
        private float speed = 2.0f;

        public TrackControl()
        {
            this.DoubleBuffered = true;
            this.BorderStyle = BorderStyle.FixedSingle;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawTrack(e.Graphics);
            DrawRobot(e.Graphics);
        }

        private void DrawTrack(Graphics g)
        {
            // 轨道背景
            using (var brush = new SolidBrush(Color.DimGray))
            {
                g.FillRectangle(brush, 0, 50, this.Width, 30);
            }

            // 轨道标记线
            using (var pen = new Pen(Color.Red, 2))
            {
                g.DrawLine(pen, 0, 65, this.Width, 65);
            }
        }

        private void DrawRobot(Graphics g)
        {
            // 机械臂本体
            var armPath = new GraphicsPath();
            armPath.AddLine(robotPosition, new PointF(robotPosition.X + 40, robotPosition.Y));
            armPath.AddLine(robotPosition, new PointF(robotPosition.X, robotPosition.Y + 30));
            armPath.CloseFigure();

            using (var brush = new SolidBrush(Color.CadetBlue))
            {
                g.FillPath(brush, armPath);
            }

            // 关节
            using (var pen = new Pen(Color.Black, 3))
            {
                g.DrawEllipse(pen, robotPosition.X - 5, robotPosition.Y - 5, 10, 10);
            }
        }

        public void UpdateRobotPosition()
        {
            // 平滑移动算法
            float dx = targetPosition.X - robotPosition.X;
            float dy = targetPosition.Y - robotPosition.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 1)
            {
                robotPosition.X += dx / distance * speed;
                robotPosition.Y += dy / distance * speed;
            }

            // 随机更新目标位置
            if (new Random().NextDouble() < 0.02)
            {
                targetPosition = new PointF(
                    100 + new Random().Next(0, this.Width - 200),
                    50 + new Random().Next(0, this.Height - 100)
                );
            }
        }
    }
}
