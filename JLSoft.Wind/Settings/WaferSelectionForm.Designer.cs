namespace JLSoft.Wind.Settings
{
    partial class WaferSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 窗体设置
            this.Text = "Wafer 类型选择";
            this.ClientSize = new Size(400, 320); // 增加窗体大小以适应更大的字体
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // 主面板
            panel1 = new Panel();
            panel1.Dock = DockStyle.Fill;
            panel1.Padding = new Padding(25); // 增加内边距
            this.Controls.Add(panel1);

            // 标题标签 - 增大字体
            label1 = new Label();
            label1.Text = "请选择Wafer类型和尺寸";
            label1.Font = new Font("微软雅黑", 14F, FontStyle.Bold); // 增大字体
            label1.AutoSize = true;
            label1.Location = new Point(70, 20);
            label1.ForeColor = Color.DarkBlue;
            panel1.Controls.Add(label1);

            // Wafer类型选择组 - 增大字体和控件尺寸
            groupBox1 = new GroupBox();
            groupBox1.Text = "Wafer类型";
            groupBox1.Location = new Point(40, 70);
            groupBox1.Size = new Size(320, 80);
            groupBox1.Font = new Font("微软雅黑", 11F, FontStyle.Regular); // 增大字体

            rbWafer = new RadioButton();
            rbWafer.Text = "Wafer";
            rbWafer.Location = new Point(25, 35);
            rbWafer.Checked = true;
            rbWafer.Font = new Font("微软雅黑", 11F, FontStyle.Regular); // 增大字体
            rbWafer.Size = new Size(100, 25);

            rbSquare = new RadioButton();
            rbSquare.Text = "Square";
            rbSquare.Location = new Point(180, 35);
            rbSquare.Font = new Font("微软雅黑", 11F, FontStyle.Regular); // 增大字体
            rbSquare.Size = new Size(100, 25);

            groupBox1.Controls.Add(rbWafer);
            groupBox1.Controls.Add(rbSquare);
            panel1.Controls.Add(groupBox1);

            // 尺寸选择组 - 增大字体和控件尺寸
            groupBox2 = new GroupBox();
            groupBox2.Text = "尺寸";
            groupBox2.Location = new Point(40, 160);
            groupBox2.Size = new Size(320, 80);
            groupBox2.Font = new Font("微软雅黑", 11F, FontStyle.Regular); // 增大字体

            rb8Inch = new RadioButton();
            rb8Inch.Text = "8英寸";
            rb8Inch.Location = new Point(25, 35);
            rb8Inch.Checked = true;
            rb8Inch.Font = new Font("微软雅黑", 11F, FontStyle.Regular); // 增大字体
            rb8Inch.Size = new Size(100, 25);

            rb12Inch = new RadioButton();
            rb12Inch.Text = "12英寸";
            rb12Inch.Location = new Point(180, 35);
            rb12Inch.Font = new Font("微软雅黑", 11F, FontStyle.Regular); // 增大字体
            rb12Inch.Size = new Size(100, 25);

            groupBox2.Controls.Add(rb8Inch);
            groupBox2.Controls.Add(rb12Inch);
            panel1.Controls.Add(groupBox2);

            // 按钮面板
            Panel buttonPanel = new Panel();
            buttonPanel.Size = new Size(320, 50);
            buttonPanel.Location = new Point(40, 250);

            // 确定按钮 - 增大字体和按钮尺寸
            btnOK = new Button();
            btnOK.Text = "确定";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(80, 10);
            btnOK.Size = new Size(90, 35);
            btnOK.Font = new Font("微软雅黑", 11F, FontStyle.Bold); // 增大字体
            btnOK.BackColor = Color.LightGreen;
            btnOK.Click += new EventHandler(btnOK_Click);
            buttonPanel.Controls.Add(btnOK);

            // 取消按钮 - 增大字体和按钮尺寸
            btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(190, 10);
            btnCancel.Size = new Size(90, 35);
            btnCancel.Font = new Font("微软雅黑", 11F, FontStyle.Bold); // 增大字体
            btnCancel.BackColor = Color.LightCoral;
            btnCancel.Click += new EventHandler(btnCancel_Click);
            buttonPanel.Controls.Add(btnCancel);

            panel1.Controls.Add(buttonPanel);

            // 设置接受和取消按钮
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // 确保窗体大小合适
            this.AutoScaleMode = AutoScaleMode.Font;
            this.PerformLayout();
        }

        #endregion
    }
}