using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.UserControl
{
    public class PrecisionArmControl : Control
    {
        // 实物特征参数
        private float _position = 0.5f;
        private bool _isEngaged;
        private const int MaterialThickness = 8;

        // 精确匹配第三张图的参数
        private const float BaseWidth = 80f;
        private const float ArmHeight = 55f;
        private const float TipSpread = 35f;

        public PrecisionArmControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(600, 300);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawIndustrialTrack(g);
            DrawMechanicalArm(g);
        }

        private void DrawIndustrialTrack(Graphics g)
        {
            // 匹配第二张图的Y型支撑结构
            int trackY = Height / 2;
            using (var trackPen = new Pen(Color.FromArgb(120, 120, 125), 12))
            {
                // Y型支撑臂
                g.DrawLine(trackPen, Width / 2 - 40, trackY - 30, Width / 2, trackY);
                g.DrawLine(trackPen, Width / 2 + 40, trackY - 30, Width / 2, trackY);

                // 水平轨道（第三张图特征）
                g.DrawLine(trackPen, 50, trackY, Width - 50, trackY);
            }
        }

        private void DrawMechanicalArm(Graphics g)
        {
            float baseX = 50 + (Width - 100) * _position;
            float baseY = Height / 2;

            // 金属支柱（第一张图垂直元素+第三张图材质）
            using (var pillarBrush = new LinearGradientBrush(
                new PointF(baseX, baseY - 70),
                new PointF(baseX, baseY),
                Color.FromArgb(80, 80, 85),
                Color.FromArgb(110, 110, 115)))
            {
                g.FillRectangle(pillarBrush,
                    baseX - 12, baseY - 70,
                    24, 70);
            }

            // 精密U型叉（精确还原第三张图）
            DrawPrecisionFork(g, baseX, baseY - 70);
        }

        private void DrawPrecisionFork(Graphics g, float pivotX, float pivotY)
        {
            // U型叉几何参数（单位：像素）
            float[] profile = {
            -TipSpread, 0,
            -BaseWidth/2, ArmHeight,
            BaseWidth/2, ArmHeight,
            TipSpread, 0
        };

            // 构建路径（带2°倾角）
            GraphicsPath path = new GraphicsPath();
            path.AddLine(pivotX + profile[0], pivotY + profile[1],
                        pivotX + profile[2], pivotY + profile[3]);
            path.AddLine(pivotX + profile[2], pivotY + profile[3],
                        pivotX + profile[4], pivotY + profile[5]);
            path.AddLine(pivotX + profile[4], pivotY + profile[5],
                        pivotX + profile[6], pivotY + profile[7]);

            // 金属质感绘制（三次渲染）
            using (var basePen = new Pen(Color.FromArgb(70, 70, 75), MaterialThickness))
            using (var highlightPen = new Pen(Color.FromArgb(150, 150, 155), 2))
            {
                // 主体结构
                g.DrawPath(basePen, path);

                // 棱边高光
                g.DrawLine(highlightPen,
                    pivotX + profile[2], pivotY + profile[3] - 2,
                    pivotX + profile[4], pivotY + profile[5] - 2);

                // 表面纹理（每5px一个刻度）
                for (float y = pivotY + 10; y < pivotY + ArmHeight; y += 5)
                {
                    g.DrawLine(Pens.DimGray,
                        pivotX - BaseWidth / 2 + 4, y,
                        pivotX + BaseWidth / 2 - 4, y);
                }
            }
        }

        // 运动控制接口
        public void SetPosition(float normalizedPos)
        {
            _position = Math.Clamp(normalizedPos, 0f, 1f);
            Invalidate();
        }
    }
}
