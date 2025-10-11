using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Settings
{
    public partial class FormMessageBox : Form
    {
        public void SetMessage(string title, string message, MessageType type)
        {
            labelTitle.Text = title;
            labelMessage.Text = message;

            // 根据不同类型设置样式
            switch (type)
            {
                case MessageType.Info:
                    panelTitleBar.BackColor = Color.FromArgb(0, 120, 215); // 蓝色
                    //pictureBoxIcon.Image = Properties.Resources.Info_icon; // 信息图标
                    break;
                case MessageType.Warning:
                    panelTitleBar.BackColor = Color.FromArgb(255, 165, 0); // 橙色
                    //pictureBoxIcon.Image = Properties.Resources.Warning_icon; // 警告图标
                    break;
                case MessageType.Error:
                    panelTitleBar.BackColor = Color.FromArgb(215, 0, 0); // 红色
                    //pictureBoxIcon.Image = Properties.Resources.Error_icon; // 错误图标
                    break;
                case MessageType.Success:
                    panelTitleBar.BackColor = Color.FromArgb(0, 150, 0); // 绿色
                    //pictureBoxIcon.Image = Properties.Resources.Success_icon; // 成功图标
                    break;
                default:
                    break;
            }

            btnClose.Click += (s, e) => this.Close();
            // 4. 确定按钮事件
            btnOk.Click += (s, e) => this.Close();
        }

        // 也可以直接在构造函数中实现
        public FormMessageBox(string title, string message, MessageType type)
        {
            InitializeComponent();
            this.SetMessage(title, message, type);


            // 应用圆角效果
            RoundTitleBar();
            RoundButton(btnOk, 10);
            RoundButton(btnClose, 15); // 关闭按钮更圆

            // 设置窗体透明背景
            TransparencyKey = BackColor;
            FormBorderStyle = FormBorderStyle.None;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private const int CornerRadius = 15;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制圆角边框
            using (var path = GetRoundRectPath(ClientRectangle, CornerRadius))
            using (var pen = new Pen(panelTitleBar.BackColor, 2))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // 左上角
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // 右上角
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // 右下角
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // 左下角
            path.CloseFigure();
            return path;
        }

        private void RoundTitleBar()
        {
            var path = new GraphicsPath();
            path.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90); // 左上角
            path.AddArc(panelTitleBar.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90); // 右上角
            path.AddLine(panelTitleBar.Width, panelTitleBar.Height, 0, panelTitleBar.Height); // 底部直线
            panelTitleBar.Region = new Region(path);
        }


        private void RoundButton(Button btn, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
            btn.Region = new Region(path);
        }
    }

    public static class CustomMessageBox
    {
        public static void Show(string message, string title, MessageType type)
        {
            Form form = new FormMessageBox(title, message, type);
            form.ShowDialog();
        }
    }
}
