using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JLSoft.Wind.Settings
{
    public partial class QtimeInFo : Form
    {
        public string InputValue { get; private set; }

        public QtimeInFo(string title, string defaultValue)
        {
            this.Text = title;
            this.Size = new Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var textBox = new TextBox
            {
                Text = defaultValue,
                Location = new Point(20, 40),
                Width = 250
            };

            var okButton = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(50, 80),
                Width = 80
            };

            var cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(150, 80),
                Width = 80
            };

            okButton.Click += (s, e) =>
            {
                InputValue = textBox.Text;
                this.Close();
            };

            this.Controls.Add(new Label
            {
                Text = "请输入Qtime值:",
                Location = new Point(20, 15)
            });

            this.Controls.Add(textBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}
