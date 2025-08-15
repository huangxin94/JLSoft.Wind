using System.Drawing.Drawing2D;


namespace JLSoft.Wind.UserControl
{


    public class RoboticArm : Control
    {
        // 定义“丫杈”的属性
        private float yachaAngle1 = 0;
        private float yachaAngle2 = 0;
        // 整体的X坐标，用于控制左右移动
        private float controlX = 0;
        // 整体的Y坐标，用于控制上下移动
        private float controlY = 0;
        // 控件的原始宽度和高度，用于拉伸计算
        private float originalWidth = 50;
        private float originalHeight = 50;

        public RoboticArm()
        {
            // 设置控件的初始大小
            this.Width = (int)originalWidth;
            this.Height = (int)originalHeight;
            // 开启双缓冲，防止绘图闪烁
            this.DoubleBuffered = true;
        }

        // 重写OnPaint方法进行绘图
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 计算当前控件缩放比例
            float scaleX = (float)this.Width / originalWidth;
            float scaleY = (float)this.Height / originalHeight;

            // 绘制绿色圆形
            float circleX = controlX * scaleX + (this.Width - 20 * scaleX) / 2;
            float circleY = controlY * scaleY + (this.Height - 20 * scaleY) / 2;
            g.FillEllipse(Brushes.Green, circleX, circleY, 20 * scaleX, 20 * scaleX);

            // 绘制紫红色矩形（居中在圆形顶部）
            float rectX = circleX + (20 * scaleX - 10 * scaleX) / 2;
            float rectY = circleY;
            g.FillRectangle(Brushes.Purple, rectX, rectY, 10 * scaleX, 5 * scaleX);

            // 绘制第一个“丫杈”
            using (GraphicsPath path1 = new GraphicsPath())
            {
                float startX = rectX + 5 * scaleX;
                float startY = rectY + 5 * scaleY;
                float endX = startX + 5 * scaleX * (float)Math.Cos(yachaAngle1);
                float endY = startY + 5 * scaleY * (float)Math.Sin(yachaAngle1);
                path1.AddLine(startX, startY, endX, endY);
                g.FillPath(Brushes.Black, path1);
            }

            // 绘制第二个“丫杈”
            using (GraphicsPath path2 = new GraphicsPath())
            {
                float startX = rectX;
                float startY = rectY + 5 * scaleY;
                float endX = startX + 5 * scaleX * (float)Math.Cos(yachaAngle2);
                float endY = startY + 5 * scaleY * (float)Math.Sin(yachaAngle2);
                path2.AddLine(startX, startY, endX, endY);
                g.FillPath(Brushes.Black, path2);
            }
        }

        // 提供方法用于转动“丫杈1”
        public void RotateYacha1(float angleDelta)
        {
            yachaAngle1 += angleDelta;
            this.Invalidate();
        }

        // 提供方法用于转动“丫杈2”
        public void RotateYacha2(float angleDelta)
        {
            yachaAngle2 += angleDelta;
            this.Invalidate();
        }

        // 提供方法用于左右移动控件
        public void MoveHorizontally(float xDelta)
        {
            controlX += xDelta;
            this.Invalidate();
        }

        // 提供方法用于上下移动控件
        public void MoveVertically(float yDelta)
        {
            controlY += yDelta;
            this.Invalidate();
        }
    }
}