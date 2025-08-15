namespace JLSoft.Wind.Settings
{
    partial class ManagementUsersFrm
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            splitter1 = new Splitter();
            panel1 = new Panel();
            uiButton1 = new Sunny.UI.UIButton();
            txt_userid = new Sunny.UI.UITextBox();
            uiLabel2 = new Sunny.UI.UILabel();
            txt_username = new Sunny.UI.UITextBox();
            uiLabel1 = new Sunny.UI.UILabel();
            uiGroupBox1 = new Sunny.UI.UIGroupBox();
            in_grade = new Sunny.UI.UIComboBox();
            uiButton2 = new Sunny.UI.UIButton();
            uiLabel5 = new Sunny.UI.UILabel();
            in_password = new Sunny.UI.UITextBox();
            uiLabel6 = new Sunny.UI.UILabel();
            in_userid = new Sunny.UI.UITextBox();
            uiLabel3 = new Sunny.UI.UILabel();
            in_username = new Sunny.UI.UITextBox();
            uiLabel4 = new Sunny.UI.UILabel();
            splitter2 = new Splitter();
            uiGroupBox2 = new Sunny.UI.UIGroupBox();
            uiDataGridView1 = new Sunny.UI.UIDataGridView();
            bindingSource1 = new BindingSource(components);
            panel1.SuspendLayout();
            uiGroupBox1.SuspendLayout();
            uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)uiDataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // splitter1
            // 
            splitter1.Dock = DockStyle.Top;
            splitter1.Location = new Point(0, 48);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(744, 3);
            splitter1.TabIndex = 1;
            splitter1.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(uiButton1);
            panel1.Controls.Add(txt_userid);
            panel1.Controls.Add(uiLabel2);
            panel1.Controls.Add(txt_username);
            panel1.Controls.Add(uiLabel1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(744, 48);
            panel1.TabIndex = 0;
            // 
            // uiButton1
            // 
            uiButton1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton1.Location = new Point(511, 7);
            uiButton1.MinimumSize = new Size(1, 1);
            uiButton1.Name = "uiButton1";
            uiButton1.Size = new Size(100, 35);
            uiButton1.TabIndex = 4;
            uiButton1.Text = "查询";
            uiButton1.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton1.Click += uiButton1_Click;
            // 
            // txt_userid
            // 
            txt_userid.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txt_userid.Location = new Point(331, 13);
            txt_userid.Margin = new Padding(4, 5, 4, 5);
            txt_userid.MinimumSize = new Size(1, 16);
            txt_userid.Name = "txt_userid";
            txt_userid.Padding = new Padding(5);
            txt_userid.ShowText = false;
            txt_userid.Size = new Size(150, 29);
            txt_userid.TabIndex = 3;
            txt_userid.TextAlignment = ContentAlignment.MiddleLeft;
            txt_userid.Watermark = "";
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(249, 19);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(75, 23);
            uiLabel2.TabIndex = 2;
            uiLabel2.Text = "用户名：";
            // 
            // txt_username
            // 
            txt_username.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txt_username.Location = new Point(90, 13);
            txt_username.Margin = new Padding(4, 5, 4, 5);
            txt_username.MinimumSize = new Size(1, 16);
            txt_username.Name = "txt_username";
            txt_username.Padding = new Padding(5);
            txt_username.ShowText = false;
            txt_username.Size = new Size(150, 29);
            txt_username.TabIndex = 1;
            txt_username.TextAlignment = ContentAlignment.MiddleLeft;
            txt_username.Watermark = "";
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(24, 19);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(59, 23);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "姓名：";
            // 
            // uiGroupBox1
            // 
            uiGroupBox1.Controls.Add(in_grade);
            uiGroupBox1.Controls.Add(uiButton2);
            uiGroupBox1.Controls.Add(uiLabel5);
            uiGroupBox1.Controls.Add(in_password);
            uiGroupBox1.Controls.Add(uiLabel6);
            uiGroupBox1.Controls.Add(in_userid);
            uiGroupBox1.Controls.Add(uiLabel3);
            uiGroupBox1.Controls.Add(in_username);
            uiGroupBox1.Controls.Add(uiLabel4);
            uiGroupBox1.Dock = DockStyle.Bottom;
            uiGroupBox1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox1.Location = new Point(0, 312);
            uiGroupBox1.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox1.MinimumSize = new Size(1, 1);
            uiGroupBox1.Name = "uiGroupBox1";
            uiGroupBox1.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox1.Size = new Size(744, 138);
            uiGroupBox1.TabIndex = 2;
            uiGroupBox1.Text = "添加用户";
            uiGroupBox1.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // in_grade
            // 
            in_grade.DataSource = null;
            in_grade.FillColor = Color.White;
            in_grade.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            in_grade.ItemHoverColor = Color.FromArgb(155, 200, 255);
            in_grade.Items.AddRange(new object[] { "普通用户", "工程师", "超级管理员" });
            in_grade.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            in_grade.Location = new Point(394, 81);
            in_grade.Margin = new Padding(4, 5, 4, 5);
            in_grade.MinimumSize = new Size(63, 0);
            in_grade.Name = "in_grade";
            in_grade.Padding = new Padding(0, 0, 30, 2);
            in_grade.Size = new Size(150, 29);
            in_grade.SymbolSize = 24;
            in_grade.TabIndex = 13;
            in_grade.TextAlignment = ContentAlignment.MiddleLeft;
            in_grade.Watermark = "";
            // 
            // uiButton2
            // 
            uiButton2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton2.Location = new Point(603, 55);
            uiButton2.MinimumSize = new Size(1, 1);
            uiButton2.Name = "uiButton2";
            uiButton2.Size = new Size(100, 35);
            uiButton2.TabIndex = 12;
            uiButton2.Text = "添加";
            uiButton2.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton2.Click += uiButton2_Click;
            // 
            // uiLabel5
            // 
            uiLabel5.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel5.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel5.Location = new Point(312, 87);
            uiLabel5.Name = "uiLabel5";
            uiLabel5.Size = new Size(75, 23);
            uiLabel5.TabIndex = 10;
            uiLabel5.Text = "职  阶：";
            // 
            // in_password
            // 
            in_password.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            in_password.Location = new Point(394, 35);
            in_password.Margin = new Padding(4, 5, 4, 5);
            in_password.MinimumSize = new Size(1, 16);
            in_password.Name = "in_password";
            in_password.Padding = new Padding(5);
            in_password.PasswordChar = '*';
            in_password.ShowText = false;
            in_password.Size = new Size(150, 29);
            in_password.TabIndex = 9;
            in_password.TextAlignment = ContentAlignment.MiddleLeft;
            in_password.Watermark = "";
            // 
            // uiLabel6
            // 
            uiLabel6.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel6.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel6.Location = new Point(312, 41);
            uiLabel6.Name = "uiLabel6";
            uiLabel6.Size = new Size(75, 23);
            uiLabel6.TabIndex = 8;
            uiLabel6.Text = "密  码：";
            // 
            // in_userid
            // 
            in_userid.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            in_userid.Location = new Point(94, 81);
            in_userid.Margin = new Padding(4, 5, 4, 5);
            in_userid.MinimumSize = new Size(1, 16);
            in_userid.Name = "in_userid";
            in_userid.Padding = new Padding(5);
            in_userid.ShowText = false;
            in_userid.Size = new Size(150, 29);
            in_userid.TabIndex = 7;
            in_userid.TextAlignment = ContentAlignment.MiddleLeft;
            in_userid.Watermark = "";
            // 
            // uiLabel3
            // 
            uiLabel3.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel3.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel3.Location = new Point(12, 87);
            uiLabel3.Name = "uiLabel3";
            uiLabel3.Size = new Size(75, 23);
            uiLabel3.TabIndex = 6;
            uiLabel3.Text = "用户名：";
            // 
            // in_username
            // 
            in_username.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            in_username.Location = new Point(94, 35);
            in_username.Margin = new Padding(4, 5, 4, 5);
            in_username.MinimumSize = new Size(1, 16);
            in_username.Name = "in_username";
            in_username.Padding = new Padding(5);
            in_username.ShowText = false;
            in_username.Size = new Size(150, 29);
            in_username.TabIndex = 5;
            in_username.TextAlignment = ContentAlignment.MiddleLeft;
            in_username.Watermark = "";
            // 
            // uiLabel4
            // 
            uiLabel4.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel4.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel4.Location = new Point(12, 41);
            uiLabel4.Name = "uiLabel4";
            uiLabel4.Size = new Size(75, 23);
            uiLabel4.TabIndex = 4;
            uiLabel4.Text = "姓  名：";
            // 
            // splitter2
            // 
            splitter2.Dock = DockStyle.Bottom;
            splitter2.Location = new Point(0, 309);
            splitter2.Name = "splitter2";
            splitter2.Size = new Size(744, 3);
            splitter2.TabIndex = 3;
            splitter2.TabStop = false;
            // 
            // uiGroupBox2
            // 
            uiGroupBox2.Controls.Add(uiDataGridView1);
            uiGroupBox2.Dock = DockStyle.Fill;
            uiGroupBox2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox2.Location = new Point(0, 51);
            uiGroupBox2.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox2.MinimumSize = new Size(1, 1);
            uiGroupBox2.Name = "uiGroupBox2";
            uiGroupBox2.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox2.Size = new Size(744, 258);
            uiGroupBox2.TabIndex = 4;
            uiGroupBox2.Text = "用户信息";
            uiGroupBox2.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // uiDataGridView1
            // 
            uiDataGridView1.AllowUserToAddRows = false;
            uiDataGridView1.AllowUserToDeleteRows = false;
            uiDataGridView1.AllowUserToResizeColumns = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            uiDataGridView1.BackgroundColor = Color.White;
            uiDataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            uiDataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            uiDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            uiDataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            uiDataGridView1.Dock = DockStyle.Fill;
            uiDataGridView1.EnableHeadersVisualStyles = false;
            uiDataGridView1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiDataGridView1.GridColor = Color.FromArgb(80, 160, 255);
            uiDataGridView1.Location = new Point(0, 32);
            uiDataGridView1.Name = "uiDataGridView1";
            uiDataGridView1.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            uiDataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiDataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            uiDataGridView1.SelectedIndex = -1;
            uiDataGridView1.Size = new Size(744, 226);
            uiDataGridView1.StripeOddColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.TabIndex = 0;
            // 
            // ManagementUsersFrm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(744, 450);
            Controls.Add(uiGroupBox2);
            Controls.Add(splitter2);
            Controls.Add(uiGroupBox1);
            Controls.Add(splitter1);
            Controls.Add(panel1);
            Name = "ManagementUsersFrm";
            Text = "用户管理";
            panel1.ResumeLayout(false);
            uiGroupBox1.ResumeLayout(false);
            uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)uiDataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Splitter splitter1;
        private Panel panel1;
        private Sunny.UI.UIButton uiButton1;
        private Sunny.UI.UITextBox txt_userid;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UITextBox txt_username;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Splitter splitter2;
        private Sunny.UI.UIGroupBox uiGroupBox2;
        private BindingSource bindingSource1;
        private Sunny.UI.UILabel uiLabel5;
        private Sunny.UI.UITextBox in_password;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UITextBox in_userid;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UITextBox in_username;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UIButton uiButton2;
        private Sunny.UI.UIComboBox in_grade;
        private Sunny.UI.UIDataGridView uiDataGridView1;
    }
}