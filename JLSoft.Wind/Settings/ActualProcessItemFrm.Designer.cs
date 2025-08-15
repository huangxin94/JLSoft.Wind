namespace JLSoft.Wind.Settings
{
    partial class ActualProcessItemFrm
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
            uiGroupBox1 = new Sunny.UI.UIGroupBox();
            but_deleteitem = new Sunny.UI.UIButton();
            uiButton2 = new Sunny.UI.UIButton();
            uiButton1 = new Sunny.UI.UIButton();
            uiGroupBox2 = new Sunny.UI.UIGroupBox();
            dataGridView1 = new DataGridView();
            uiGroupBox1.SuspendLayout();
            uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // uiGroupBox1
            // 
            uiGroupBox1.Controls.Add(but_deleteitem);
            uiGroupBox1.Controls.Add(uiButton2);
            uiGroupBox1.Controls.Add(uiButton1);
            uiGroupBox1.Dock = DockStyle.Top;
            uiGroupBox1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox1.Location = new Point(0, 0);
            uiGroupBox1.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox1.MinimumSize = new Size(1, 1);
            uiGroupBox1.Name = "uiGroupBox1";
            uiGroupBox1.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox1.Size = new Size(800, 77);
            uiGroupBox1.TabIndex = 0;
            uiGroupBox1.Text = "操作";
            uiGroupBox1.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // but_deleteitem
            // 
            but_deleteitem.FillColor = Color.FromArgb(255, 128, 128);
            but_deleteitem.FillColor2 = Color.FromArgb(255, 128, 128);
            but_deleteitem.FillHoverColor = Color.FromArgb(255, 192, 192);
            but_deleteitem.FillPressColor = Color.Red;
            but_deleteitem.FillSelectedColor = Color.Red;
            but_deleteitem.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_deleteitem.Location = new Point(264, 35);
            but_deleteitem.MinimumSize = new Size(1, 1);
            but_deleteitem.Name = "but_deleteitem";
            but_deleteitem.RectColor = Color.FromArgb(255, 128, 128);
            but_deleteitem.RectHoverColor = Color.FromArgb(255, 128, 128);
            but_deleteitem.RectPressColor = Color.Red;
            but_deleteitem.RectSelectedColor = Color.Red;
            but_deleteitem.Size = new Size(90, 29);
            but_deleteitem.TabIndex = 33;
            but_deleteitem.Text = "删除";
            but_deleteitem.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // uiButton2
            // 
            uiButton2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton2.Location = new Point(133, 35);
            uiButton2.MinimumSize = new Size(1, 1);
            uiButton2.Name = "uiButton2";
            uiButton2.Size = new Size(90, 29);
            uiButton2.TabIndex = 1;
            uiButton2.Text = "下移";
            uiButton2.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // uiButton1
            // 
            uiButton1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton1.Location = new Point(12, 35);
            uiButton1.MinimumSize = new Size(1, 1);
            uiButton1.Name = "uiButton1";
            uiButton1.Size = new Size(90, 29);
            uiButton1.TabIndex = 0;
            uiButton1.Text = "上移";
            uiButton1.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // uiGroupBox2
            // 
            uiGroupBox2.Controls.Add(dataGridView1);
            uiGroupBox2.Dock = DockStyle.Fill;
            uiGroupBox2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox2.Location = new Point(0, 77);
            uiGroupBox2.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox2.MinimumSize = new Size(1, 1);
            uiGroupBox2.Name = "uiGroupBox2";
            uiGroupBox2.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox2.Size = new Size(800, 373);
            uiGroupBox2.TabIndex = 1;
            uiGroupBox2.Text = "数据展示";
            uiGroupBox2.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 32);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.Size = new Size(800, 341);
            dataGridView1.TabIndex = 0;
            // 
            // ActualProcessItemFrm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(uiGroupBox2);
            Controls.Add(uiGroupBox1);
            Name = "ActualProcessItemFrm";
            Text = "ActualProcessItemFrm";
            Load += ActualProcessItemFrm_Load;
            uiGroupBox1.ResumeLayout(false);
            uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UIButton uiButton2;
        private Sunny.UI.UIButton uiButton1;
        private Sunny.UI.UIButton but_deleteitem;
        private Sunny.UI.UIGroupBox uiGroupBox2;
        private DataGridView dataGridView1;
    }
}