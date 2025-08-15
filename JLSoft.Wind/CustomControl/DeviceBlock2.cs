using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace JLSoft.Wind.CustomControl
{
    [DesignerCategory("Code")]
    public class DeviceBlock2 : System.Windows.Forms.UserControl
    {
        // 添加标准Text属性支持
        [Browsable(true), Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => DeviceCode; // 默认将Text映射到设备代码
            set
            {
                if (DeviceCode != value)
                {
                    DeviceCode = value;
                    Invalidate(); // 重绘控件
                }
            }
        }

        // 设备编码属性
        [Browsable(true), Category("设备属性")]
        public string DeviceCode { get; set; } = "UV CURE A";

        // 状态值属性
        [Browsable(true), Category("设备属性")]
        public string StatusValue { get; set; } = "7";

        // 背景色属性
        [Browsable(true), Category("设备属性")]
        public Color BlockColor { get; set; } = Color.FromArgb(144, 238, 144);

        // 设备编码字体属性
        private Font _deviceCodeFont = new Font("Arial", 10, FontStyle.Bold);
        [Browsable(true), Category("设备属性")]
        public Font DeviceCodeFont
        {
            get => _deviceCodeFont;
            set
            {
                _deviceCodeFont = value;
                Invalidate();
            }
        }

        // 状态值字体属性
        private Font _statusValueFont = new Font("Arial", 16, FontStyle.Bold);
        [Browsable(true), Category("设备属性")]
        public Font StatusValueFont
        {
            get => _statusValueFont;
            set
            {
                _statusValueFont = value;
                Invalidate();
            }
        }

        // 状态值垂直偏移量属性
        [Browsable(true), Category("设备属性")]
        [DefaultValue(0)]
        public int StatusValueOffsetY { get; set; } = 0;

        public DeviceBlock2()
        {
            Size = new Size(150, 80);
            DoubleBuffered = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _deviceCodeFont?.Dispose();
                _statusValueFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制背景
            using (var brush = new SolidBrush(BlockColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            e.Graphics.DrawRectangle(Pens.DarkGray, 0, 0, Width - 1, Height - 1);

            // 使用TextRenderer绘制文本（兼容标准Windows控件渲染）

            // 设备编码（顶部居中）
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top;
            TextRenderer.DrawText(
                e.Graphics,
                DeviceCode, // 使用DeviceCode属性
                DeviceCodeFont,
                new Rectangle(0, 5, Width, 20),
                ForeColor, // 使用ForeColor属性
                flags
            );

            // 状态值（垂直水平居中 + 可调偏移）
            int statusY = 25 + StatusValueOffsetY;
            Rectangle statusRect = new Rectangle(0, statusY, Width, Height - statusY);

            TextRenderer.DrawText(
                e.Graphics,
                StatusValue, // 使用StatusValue属性
                StatusValueFont,
                statusRect,
                ForeColor, // 使用ForeColor属性
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }
    }
}