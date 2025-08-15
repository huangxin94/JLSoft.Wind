using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Class
{
    public class ProcessLineManager2
    {
        private readonly List<Tuple<Point, Point>> _connections = new List<Tuple<Point, Point>>();
        private readonly Control _canvas;

        public ProcessLineManager2(Control canvas) => _canvas = canvas;

        // 添加连接
        public void AddConnection(Control source, Control target)
        {
            var start = new Point(source.Right, source.Top + source.Height / 2);
            var end = new Point(target.Left, target.Top + target.Height / 2);
            _connections.Add(Tuple.Create(start, end));
            _canvas.Invalidate();
        }

        // 绘制所有连接线
        public void DrawLines(Graphics g)
        {
            foreach (var conn in _connections)
            {
                using (var pen = new Pen(Color.SteelBlue, 2))
                {
                    // 绘制带箭头的直线
                    g.DrawLine(pen, conn.Item1, conn.Item2);
                    DrawArrow(g, pen, conn.Item2, conn.Item1);
                }
            }
        }

        private void DrawArrow(Graphics g, Pen pen, Point end, Point start)
        {
            // 箭头绘制逻辑（基于角度计算）
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            Point[] arrowPoints = {
            end,
            new Point(end.X - 10, end.Y - 5),
            new Point(end.X - 10, end.Y + 5)
        };
            g.FillPolygon(pen.Brush, arrowPoints);
        }
    }
}
