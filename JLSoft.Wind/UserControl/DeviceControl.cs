using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace JLSoft.Wind.UserControl
{


    public class DeviceControl : Control
    {
        private Color _cubeColor = Color.Green;
        private string _labelText = "设备A\n正常";

        public DeviceControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true; // 避免闪烁
        }

        [Browsable(true)]
        public Color CubeColor
        {
            get => _cubeColor;
            set
            {
                _cubeColor = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制立体盒子
            DrawCube(e.Graphics, ClientRectangle, _cubeColor);

            // 绘制文字标签
            using (var brush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(_labelText, Font, brush, ClientRectangle);
            }
        }

        private void DrawCube(Graphics g, Rectangle rect, Color color)
        {
            using (var pen = new Pen(color))
            {
                // 前面
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

                // 透视效果（伪 3D）
                int depth = 5;
                Rectangle backRect = new Rectangle(
                    rect.X + depth,
                    rect.Y + depth,
                    rect.Width - depth * 2,
                    rect.Height - depth * 2);
                g.DrawRectangle(pen, backRect);

                // 连接线（简化版）
                g.DrawLine(pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
                g.DrawLine(pen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
            }
        }
    }
}