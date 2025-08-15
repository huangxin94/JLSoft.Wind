using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace JLSoft.Wind.UserControl
{
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
    [ToolboxItem(true)]
    public class PseudoCubeControl : Control
    {
        private Color _cubeColor = Color.Green;
        private string _labelText = "设备A\n正常";
        private Color _labelColor = Color.Black;
        private Font _labelFont = new Font("Arial", 12);
        private const string CachedLabelKey = "CachedLabel";

        public PseudoCubeControl()
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(100, 100); // 设置默认尺寸
        }

        [Browsable(true)]
        public Color CubeColor
        {
            get => _cubeColor;
            set { _cubeColor = value; Invalidate(); }
        }

        [Browsable(true)]
        public string LabelText
        {
            get => _labelText;
            set { _labelText = value; Invalidate(); }
        }

        [Browsable(true)]
        public Color LabelColor
        {
            get => _labelColor;
            set { _labelColor = value; Invalidate(); }
        }

        [Browsable(true)]
        public Font LabelFont
        {
            get => _labelFont;
            set { _labelFont = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            if (ClientSize.Width < 1 || ClientSize.Height < 1) return;

            DrawSquareWithDiagonals(e.Graphics);
            DrawLabel(e.Graphics);
        }

        private void DrawSquareWithDiagonals(Graphics g)
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;
            using (Pen pen = new Pen(_cubeColor))
            {
                // 绘制正方形边框
                g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
                // 绘制两条对角线
                g.DrawLine(pen, 0, 0, width - 1, height - 1);
                g.DrawLine(pen, width - 1, 0, 0, height - 1);
            }
        }

        private void DrawLabel(Graphics g)
        {
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            })
            using (var brush = new SolidBrush(_labelColor))
            {
                g.DrawString(_labelText, _labelFont, brush, new PointF(0, 0), format);
            }
        }
    }
}