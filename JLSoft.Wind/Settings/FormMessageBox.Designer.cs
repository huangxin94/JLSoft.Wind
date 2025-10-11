namespace JLSoft.Wind.Settings
{
    partial class FormMessageBox
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
            panelTitleBar = new Panel();
            btnClose = new Button();
            labelTitle = new Label();
            labelMessage = new Label();
            panel1 = new Panel();
            btnOk = new Button();
            panelTitleBar.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panelTitleBar
            // 
            panelTitleBar.BackColor = Color.Gold;
            panelTitleBar.Controls.Add(btnClose);
            panelTitleBar.Controls.Add(labelTitle);
            panelTitleBar.Dock = DockStyle.Top;
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Margin = new Padding(3, 2, 3, 2);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(384, 40);
            panelTitleBar.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.FlatAppearance.BorderColor = Color.Black;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Arial Black", 14F);
            btnClose.ForeColor = Color.Red;
            btnClose.Location = new Point(345, 1);
            btnClose.Margin = new Padding(3, 2, 3, 2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(37, 37);
            btnClose.TabIndex = 1;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("幼圆", 16F);
            labelTitle.ForeColor = Color.Black;
            labelTitle.Location = new Point(3, 9);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(54, 22);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "警告";
            // 
            // labelMessage
            // 
            labelMessage.Font = new Font("幼圆", 14F);
            labelMessage.Location = new Point(0, 91);
            labelMessage.Name = "labelMessage";
            labelMessage.RightToLeft = RightToLeft.No;
            labelMessage.Size = new Size(384, 19);
            labelMessage.TabIndex = 1;
            labelMessage.Text = "PLC连接未初始化！";
            labelMessage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(btnOk);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 162);
            panel1.Name = "panel1";
            panel1.Size = new Size(384, 49);
            panel1.TabIndex = 2;
            // 
            // btnOk
            // 
            btnOk.Font = new Font("幼圆", 14F);
            btnOk.Location = new Point(140, 8);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(104, 33);
            btnOk.TabIndex = 3;
            btnOk.Text = "确定";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // FormMessageBox
            // 
            AutoScaleDimensions = new SizeF(6F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(384, 211);
            ControlBox = false;
            Controls.Add(panel1);
            Controls.Add(labelMessage);
            Controls.Add(panelTitleBar);
            Font = new Font("幼圆", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormMessageBox";
            Text = "FormMessageBox";
            panelTitleBar.ResumeLayout(false);
            panelTitleBar.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelTitleBar;
        private Label labelTitle;
        private Button btnClose;
        private Label labelMessage;
        private Panel panel1;
        private Button btnOk;
    }
}