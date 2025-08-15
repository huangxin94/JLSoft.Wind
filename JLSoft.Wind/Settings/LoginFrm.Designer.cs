namespace JLSoft.Wind.Settings
{
    partial class LoginFrm
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
            uiLabel1 = new Sunny.UI.UILabel();
            txtUsername = new Sunny.UI.UITextBox();
            txtPassword = new Sunny.UI.UITextBox();
            uiLabel2 = new Sunny.UI.UILabel();
            butLogin = new Sunny.UI.UIButton();
            SuspendLayout();
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(39, 74);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(80, 23);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "用户名：";
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtUsername.Location = new Point(126, 68);
            txtUsername.Margin = new Padding(4, 5, 4, 5);
            txtUsername.MinimumSize = new Size(1, 16);
            txtUsername.Name = "txtUsername";
            txtUsername.Padding = new Padding(5);
            txtUsername.ShowText = false;
            txtUsername.Size = new Size(150, 29);
            txtUsername.TabIndex = 1;
            txtUsername.TextAlignment = ContentAlignment.MiddleLeft;
            txtUsername.Watermark = "";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtPassword.Location = new Point(126, 145);
            txtPassword.Margin = new Padding(4, 5, 4, 5);
            txtPassword.MinimumSize = new Size(1, 16);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(5);
            txtPassword.PasswordChar = '*';
            txtPassword.ShowText = false;
            txtPassword.Size = new Size(150, 29);
            txtPassword.TabIndex = 3;
            txtPassword.TextAlignment = ContentAlignment.MiddleLeft;
            txtPassword.Watermark = "";
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(39, 151);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(80, 23);
            uiLabel2.TabIndex = 2;
            uiLabel2.Text = "密  码：";
            // 
            // butLogin
            // 
            butLogin.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            butLogin.Location = new Point(126, 220);
            butLogin.MinimumSize = new Size(1, 1);
            butLogin.Name = "butLogin";
            butLogin.Size = new Size(100, 35);
            butLogin.TabIndex = 4;
            butLogin.Text = "登   录";
            butLogin.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            butLogin.Click += butLogin_Click;
            // 
            // LoginFrm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(331, 338);
            Controls.Add(butLogin);
            Controls.Add(txtPassword);
            Controls.Add(uiLabel2);
            Controls.Add(txtUsername);
            Controls.Add(uiLabel1);
            Name = "LoginFrm";
            Text = "登录";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UITextBox txtUsername;
        private Sunny.UI.UITextBox txtPassword;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton butLogin;
    }
}