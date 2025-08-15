using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Class
{
    public class ProcessLineManager
    {
        private readonly Control _canvas;
        private readonly List<Tuple<Point, Point>> _connections = new List<Tuple<Point, Point>>();

        public void AddConnection(Control source, Control target)
        {
            // 计算控件中心点坐标
            Point sourceCenter = new Point(
                source.Location.X + source.Width / 2,
                source.Location.Y + source.Height / 2);

            Point targetCenter = new Point(
                target.Location.X + target.Width / 2,
                target.Location.Y + target.Height / 2);

            _connections.Add(Tuple.Create(sourceCenter, targetCenter));
            _canvas.Invalidate();
        }

        public void DrawConnections(Graphics g)
        {
            foreach (var conn in _connections)
            {
                using (var pen = new Pen(Color.SteelBlue, 2))
                {
                    // 绘制贝塞尔曲线
                    var ctrl1 = new Point(conn.Item1.X + 50, conn.Item1.Y);
                    var ctrl2 = new Point(conn.Item2.X - 50, conn.Item2.Y);
                    g.DrawBezier(pen, conn.Item1, ctrl1, ctrl2, conn.Item2);

                    // 绘制箭头
                    DrawArrowHead(g, pen, conn.Item2, ctrl2);
                }
            }
        }

        private void DrawArrowHead(Graphics g, Pen pen, Point end, Point ctrl)
        {
            // 箭头方向计算
            var direction = new Point(end.X - ctrl.X, end.Y - ctrl.Y);
            var arrowSize = 10;
            var points = new[]
            {
            end,
            new Point(end.X - arrowSize + direction.X/2, end.Y - arrowSize + direction.Y/2),
            new Point(end.X - arrowSize - direction.X/2, end.Y + arrowSize - direction.Y/2)
        };
            g.FillPolygon(pen.Brush, points);
        }
    }
}
