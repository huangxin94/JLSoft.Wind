using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System;
using System.Drawing.Text;

namespace JLSoft.Wind.UserControl
{
    public class IndustrialModule : Control
    {
        // 配置参数
        private const int DEPTH = 8;
        private const float CORNER_RADIUS = 6f;
        private Color _baseColor = Color.SteelBlue;
        private string _moduleName;

        // 属性与Text的绑定
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ModuleName
        {
            get => _moduleName;
            set
            {
                if (_moduleName != value)
                {
                    _moduleName = value;
                    base.Text = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get => ModuleName;
            set => ModuleName = value;
        }

        public Color BaseColor
        {
            get => _baseColor;
            set { _baseColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value ?? new Font("新宋体", 9f, FontStyle.Bold);
                Invalidate();
            }
        }

        public IndustrialModule()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                   ControlStyles.ResizeRedraw |
                   ControlStyles.SupportsTransparentBackColor,
                   true);
            Font = new Font("新宋体", 9f, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < DEPTH * 2 || Height < DEPTH * 2)
            {
                DrawSizeWarning(e.Graphics);
                return;
            }

            base.OnPaint(e);
            ConfigureGraphics(e.Graphics);

            Draw3DBody(e.Graphics);
            DrawShadows(e.Graphics);
            DrawText(e.Graphics);
        }

        #region 绘图实现
        private void ConfigureGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        private void Draw3DBody(Graphics g)
        {
            // 主体区域保持控件完整尺寸
            using (var path = CreateRoundedRect(0, 0, Width, Height, CORNER_RADIUS))
            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, Width, Height),
                BaseColor,
                ControlPaint.Dark(BaseColor, 0.3f),
                135f))
            {
                g.FillPath(brush, path);
            }
        }

        private void DrawShadows(Graphics g)
        {
            // 调整阴影绘制区域，限制在控件边界内
            using (var shadowPath = new GraphicsPath())
            {
                // 右侧阴影修正（向内缩进1像素避免覆盖边框）
                shadowPath.AddLines(new[] {
            new Point(Width - DEPTH + 1, DEPTH),
            new Point(Width - 1, 0),
            new Point(Width - 1, Height - 1),
            new Point(Width - DEPTH + 1, Height - DEPTH - 1)
        });

                // 底部阴影修正
                shadowPath.AddLines(new[] {
            new Point(Width - DEPTH + 1, Height - DEPTH - 1),
            new Point(DEPTH, Height - DEPTH - 1),
            new Point(0, Height - 1),
            new Point(Width - 1, Height - 1)
        });

                using (var compositeBrush = new PathGradientBrush(shadowPath))
                {
                    compositeBrush.CenterColor = Color.FromArgb(120, 0, 0, 0);
                    compositeBrush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(compositeBrush, shadowPath);
                }
            }
        }

        private void DrawText(Graphics g)
        {
            if (string.IsNullOrEmpty(ModuleName)) return;

            var textRect = new Rectangle(
                DEPTH,
                DEPTH,
                Width - DEPTH * 2,
                Height - DEPTH * 2);

            var fontSize = CalculateAutoFontSize(g, textRect);
            using var font = new Font(Font.FontFamily, fontSize, FontStyle.Bold);

            // 优化阴影绘制
            /*
            var shadowColor = Color.FromArgb(80, 0, 0, 0);
            TextRenderer.DrawText(
                g,
                ModuleName,
                font,
                new Rectangle(textRect.X + 2, textRect.Y + 2, textRect.Width, textRect.Height),
                shadowColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            */
            // 主文本绘制
            TextRenderer.DrawText(
                g,
                ModuleName,
                Font,
                textRect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private float CalculateAutoFontSize(Graphics g, Rectangle rect)
        {
            var proposedSize = TextRenderer.MeasureText(
                g,
                ModuleName,
                Font,
                rect.Size,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);

            float scaleFactor = Math.Min(
                rect.Width / (float)proposedSize.Width,
                rect.Height / (float)proposedSize.Height);

            return Math.Min(Font.Size, Font.Size * scaleFactor);
        }
        #endregion

        #region 辅助方法
        private void DrawSizeWarning(Graphics g)
        {
            using var warningFont = new Font("Arial", 8f);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            g.DrawString("Size too small",
                warningFont,
                Brushes.Red,
                ClientRectangle,
                format);
        }

        private GraphicsPath CreateRoundedRect(float x, float y, float w, float h, float radius)
        {
            var path = new GraphicsPath();
            float diameter = radius * 2;

            path.AddArc(x, y, diameter, diameter, 180, 90);
            path.AddArc(x + w - diameter, y, diameter, diameter, 270, 90);
            path.AddArc(x + w - diameter, y + h - diameter, diameter, diameter, 0, 90);
            path.AddArc(x, y + h - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
        #endregion
    }
}