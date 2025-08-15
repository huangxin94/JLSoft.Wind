namespace JLSoft.Wind.Settings
{
    partial class ProcessFlowFrm
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
            uiPanel1 = new Sunny.UI.UIPanel();
            uiGroupBox5 = new Sunny.UI.UIGroupBox();
            dataGridView3 = new DataGridView();
            uiGroupBox4 = new Sunny.UI.UIGroupBox();
            dataGridView2 = new DataGridView();
            uiGroupBox3 = new Sunny.UI.UIGroupBox();
            dataGridView1 = new DataGridView();
            uiGroupBox2 = new Sunny.UI.UIGroupBox();
            tbx_Qtime = new Sunny.UI.UITextBox();
            uiLabel4 = new Sunny.UI.UILabel();
            but_insert = new Sunny.UI.UIButton();
            but_deleteitem = new Sunny.UI.UIButton();
            but_addprocessitem = new Sunny.UI.UIButton();
            but_deleteprocess = new Sunny.UI.UIButton();
            but_effectlose = new Sunny.UI.UIButton();
            but_addprocess = new Sunny.UI.UIButton();
            cbx_addsite = new Sunny.UI.UIComboBox();
            uiLabel6 = new Sunny.UI.UILabel();
            tbx_addprocessname = new Sunny.UI.UITextBox();
            uiLabel3 = new Sunny.UI.UILabel();
            tbx_addmaterialname = new Sunny.UI.UITextBox();
            uiLabel2 = new Sunny.UI.UILabel();
            uiGroupBox1 = new Sunny.UI.UIGroupBox();
            but_Query = new Sunny.UI.UIButton();
            uiComboBox1 = new Sunny.UI.UIComboBox();
            uiLabel1 = new Sunny.UI.UILabel();
            txt_AxisX = new Sunny.UI.UITextBox();
            uiLabel18 = new Sunny.UI.UILabel();
            uiPanel1.SuspendLayout();
            uiGroupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView3).BeginInit();
            uiGroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            uiGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            uiGroupBox2.SuspendLayout();
            uiGroupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // uiPanel1
            // 
            uiPanel1.Controls.Add(uiGroupBox5);
            uiPanel1.Controls.Add(uiGroupBox4);
            uiPanel1.Controls.Add(uiGroupBox3);
            uiPanel1.Controls.Add(uiGroupBox2);
            uiPanel1.Controls.Add(uiGroupBox1);
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiPanel1.Location = new Point(0, 0);
            uiPanel1.Margin = new Padding(4, 5, 4, 5);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.Size = new Size(853, 629);
            uiPanel1.TabIndex = 0;
            uiPanel1.Text = null;
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiGroupBox5
            // 
            uiGroupBox5.Controls.Add(dataGridView3);
            uiGroupBox5.Dock = DockStyle.Fill;
            uiGroupBox5.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox5.Location = new Point(614, 212);
            uiGroupBox5.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox5.MinimumSize = new Size(1, 1);
            uiGroupBox5.Name = "uiGroupBox5";
            uiGroupBox5.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox5.Size = new Size(239, 417);
            uiGroupBox5.TabIndex = 4;
            uiGroupBox5.Text = "过站顺序";
            uiGroupBox5.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // dataGridView3
            // 
            dataGridView3.AllowUserToAddRows = false;
            dataGridView3.AllowUserToDeleteRows = false;
            dataGridView3.AllowUserToResizeColumns = false;
            dataGridView3.BackgroundColor = Color.White;
            dataGridView3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView3.Dock = DockStyle.Fill;
            dataGridView3.Location = new Point(0, 32);
            dataGridView3.MultiSelect = false;
            dataGridView3.Name = "dataGridView3";
            dataGridView3.ReadOnly = true;
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView3.Size = new Size(239, 385);
            dataGridView3.TabIndex = 1;
            dataGridView3.CellDoubleClick += dataGridView3_CellDoubleClick;
            // 
            // uiGroupBox4
            // 
            uiGroupBox4.Controls.Add(dataGridView2);
            uiGroupBox4.Dock = DockStyle.Left;
            uiGroupBox4.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox4.Location = new Point(484, 212);
            uiGroupBox4.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox4.MinimumSize = new Size(1, 1);
            uiGroupBox4.Name = "uiGroupBox4";
            uiGroupBox4.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox4.Size = new Size(130, 417);
            uiGroupBox4.TabIndex = 3;
            uiGroupBox4.Text = "类型";
            uiGroupBox4.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AllowUserToResizeColumns = false;
            dataGridView2.BackgroundColor = Color.White;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Dock = DockStyle.Fill;
            dataGridView2.Location = new Point(0, 32);
            dataGridView2.MultiSelect = false;
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.Size = new Size(130, 385);
            dataGridView2.TabIndex = 1;
            dataGridView2.CellClick += dataGridView2_CellClick;
            // 
            // uiGroupBox3
            // 
            uiGroupBox3.Controls.Add(dataGridView1);
            uiGroupBox3.Dock = DockStyle.Left;
            uiGroupBox3.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox3.Location = new Point(0, 212);
            uiGroupBox3.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox3.MinimumSize = new Size(1, 1);
            uiGroupBox3.Name = "uiGroupBox3";
            uiGroupBox3.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox3.Size = new Size(484, 417);
            uiGroupBox3.TabIndex = 2;
            uiGroupBox3.Text = "产品信息";
            uiGroupBox3.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 32);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(484, 385);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellClick += dataGridView1_CellClick;
            // 
            // uiGroupBox2
            // 
            uiGroupBox2.Controls.Add(tbx_Qtime);
            uiGroupBox2.Controls.Add(uiLabel4);
            uiGroupBox2.Controls.Add(but_insert);
            uiGroupBox2.Controls.Add(but_deleteitem);
            uiGroupBox2.Controls.Add(but_addprocessitem);
            uiGroupBox2.Controls.Add(but_deleteprocess);
            uiGroupBox2.Controls.Add(but_effectlose);
            uiGroupBox2.Controls.Add(but_addprocess);
            uiGroupBox2.Controls.Add(cbx_addsite);
            uiGroupBox2.Controls.Add(uiLabel6);
            uiGroupBox2.Controls.Add(tbx_addprocessname);
            uiGroupBox2.Controls.Add(uiLabel3);
            uiGroupBox2.Controls.Add(tbx_addmaterialname);
            uiGroupBox2.Controls.Add(uiLabel2);
            uiGroupBox2.Dock = DockStyle.Top;
            uiGroupBox2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox2.Location = new Point(0, 92);
            uiGroupBox2.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox2.MinimumSize = new Size(1, 1);
            uiGroupBox2.Name = "uiGroupBox2";
            uiGroupBox2.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox2.Size = new Size(853, 120);
            uiGroupBox2.TabIndex = 1;
            uiGroupBox2.Text = "添加修改";
            uiGroupBox2.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // tbx_Qtime
            // 
            tbx_Qtime.DoubleValue = 2D;
            tbx_Qtime.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tbx_Qtime.Location = new Point(326, 76);
            tbx_Qtime.Margin = new Padding(4, 5, 4, 5);
            tbx_Qtime.MinimumSize = new Size(1, 16);
            tbx_Qtime.Name = "tbx_Qtime";
            tbx_Qtime.Padding = new Padding(5);
            tbx_Qtime.ShowText = false;
            tbx_Qtime.Size = new Size(121, 29);
            tbx_Qtime.TabIndex = 35;
            tbx_Qtime.Text = "2.00";
            tbx_Qtime.TextAlignment = ContentAlignment.MiddleCenter;
            tbx_Qtime.Type = Sunny.UI.UITextBox.UIEditType.Double;
            tbx_Qtime.Watermark = "";
            // 
            // uiLabel4
            // 
            uiLabel4.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel4.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel4.Location = new Point(233, 82);
            uiLabel4.Name = "uiLabel4";
            uiLabel4.Size = new Size(86, 23);
            uiLabel4.TabIndex = 34;
            uiLabel4.Text = "Qtime:";
            uiLabel4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // but_insert
            // 
            but_insert.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_insert.Location = new Point(550, 79);
            but_insert.MinimumSize = new Size(1, 1);
            but_insert.Name = "but_insert";
            but_insert.Size = new Size(90, 29);
            but_insert.TabIndex = 33;
            but_insert.Text = "插入";
            but_insert.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_insert.Click += but_insert_Click;
            // 
            // but_deleteitem
            // 
            but_deleteitem.FillColor = Color.FromArgb(255, 128, 128);
            but_deleteitem.FillColor2 = Color.FromArgb(255, 128, 128);
            but_deleteitem.FillHoverColor = Color.FromArgb(255, 192, 192);
            but_deleteitem.FillPressColor = Color.Red;
            but_deleteitem.FillSelectedColor = Color.Red;
            but_deleteitem.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_deleteitem.Location = new Point(646, 79);
            but_deleteitem.MinimumSize = new Size(1, 1);
            but_deleteitem.Name = "but_deleteitem";
            but_deleteitem.RectColor = Color.FromArgb(255, 128, 128);
            but_deleteitem.RectHoverColor = Color.FromArgb(255, 128, 128);
            but_deleteitem.RectPressColor = Color.Red;
            but_deleteitem.RectSelectedColor = Color.Red;
            but_deleteitem.Size = new Size(90, 29);
            but_deleteitem.TabIndex = 32;
            but_deleteitem.Text = "删除";
            but_deleteitem.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_deleteitem.Click += but_deleteitem_Click;
            // 
            // but_addprocessitem
            // 
            but_addprocessitem.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_addprocessitem.Location = new Point(454, 79);
            but_addprocessitem.MinimumSize = new Size(1, 1);
            but_addprocessitem.Name = "but_addprocessitem";
            but_addprocessitem.Size = new Size(90, 29);
            but_addprocessitem.TabIndex = 31;
            but_addprocessitem.Text = "添加";
            but_addprocessitem.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_addprocessitem.Click += but_addprocessitem_Click;
            // 
            // but_deleteprocess
            // 
            but_deleteprocess.FillColor = Color.FromArgb(255, 128, 128);
            but_deleteprocess.FillColor2 = Color.FromArgb(255, 128, 128);
            but_deleteprocess.FillHoverColor = Color.FromArgb(255, 128, 128);
            but_deleteprocess.FillPressColor = Color.Red;
            but_deleteprocess.FillSelectedColor = Color.Red;
            but_deleteprocess.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_deleteprocess.Location = new Point(646, 39);
            but_deleteprocess.MinimumSize = new Size(1, 1);
            but_deleteprocess.Name = "but_deleteprocess";
            but_deleteprocess.RectColor = Color.FromArgb(255, 128, 128);
            but_deleteprocess.RectHoverColor = Color.FromArgb(255, 192, 192);
            but_deleteprocess.RectPressColor = Color.Red;
            but_deleteprocess.RectSelectedColor = Color.Red;
            but_deleteprocess.Size = new Size(90, 29);
            but_deleteprocess.TabIndex = 30;
            but_deleteprocess.Text = "删除";
            but_deleteprocess.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_deleteprocess.Click += but_deleteprocess_Click;
            // 
            // but_effectlose
            // 
            but_effectlose.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_effectlose.Location = new Point(550, 39);
            but_effectlose.MinimumSize = new Size(1, 1);
            but_effectlose.Name = "but_effectlose";
            but_effectlose.Size = new Size(90, 29);
            but_effectlose.TabIndex = 29;
            but_effectlose.Text = "生效";
            but_effectlose.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_effectlose.Click += but_effectlose_Click;
            // 
            // but_addprocess
            // 
            but_addprocess.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_addprocess.Location = new Point(454, 39);
            but_addprocess.MinimumSize = new Size(1, 1);
            but_addprocess.Name = "but_addprocess";
            but_addprocess.Size = new Size(90, 29);
            but_addprocess.TabIndex = 28;
            but_addprocess.Text = "添加";
            but_addprocess.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_addprocess.Click += but_addrocess_Click;
            // 
            // cbx_addsite
            // 
            cbx_addsite.DataSource = null;
            cbx_addsite.FillColor = Color.White;
            cbx_addsite.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            cbx_addsite.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cbx_addsite.Items.AddRange(new object[] { "A1", "A2", "A3", "A4", "C1", "G1", "R1", "M1", "U1", "V1", "S1", "S2", "S3", "角度台" });
            cbx_addsite.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cbx_addsite.Location = new Point(105, 79);
            cbx_addsite.Margin = new Padding(4, 5, 4, 5);
            cbx_addsite.MinimumSize = new Size(63, 0);
            cbx_addsite.Name = "cbx_addsite";
            cbx_addsite.Padding = new Padding(0, 0, 30, 2);
            cbx_addsite.Size = new Size(121, 29);
            cbx_addsite.SymbolSize = 24;
            cbx_addsite.TabIndex = 27;
            cbx_addsite.Text = "站点";
            cbx_addsite.TextAlignment = ContentAlignment.MiddleCenter;
            cbx_addsite.Watermark = "";
            // 
            // uiLabel6
            // 
            uiLabel6.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel6.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel6.Location = new Point(12, 85);
            uiLabel6.Name = "uiLabel6";
            uiLabel6.Size = new Size(86, 23);
            uiLabel6.TabIndex = 26;
            uiLabel6.Text = "站    点:";
            // 
            // tbx_addprocessname
            // 
            tbx_addprocessname.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tbx_addprocessname.Location = new Point(326, 39);
            tbx_addprocessname.Margin = new Padding(4, 5, 4, 5);
            tbx_addprocessname.MinimumSize = new Size(1, 16);
            tbx_addprocessname.Name = "tbx_addprocessname";
            tbx_addprocessname.Padding = new Padding(5);
            tbx_addprocessname.ShowText = false;
            tbx_addprocessname.Size = new Size(121, 29);
            tbx_addprocessname.TabIndex = 9;
            tbx_addprocessname.Text = "ABCV1";
            tbx_addprocessname.TextAlignment = ContentAlignment.MiddleCenter;
            tbx_addprocessname.Watermark = "";
            // 
            // uiLabel3
            // 
            uiLabel3.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel3.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel3.Location = new Point(233, 45);
            uiLabel3.Name = "uiLabel3";
            uiLabel3.Size = new Size(86, 23);
            uiLabel3.TabIndex = 8;
            uiLabel3.Text = "工艺名称:";
            uiLabel3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbx_addmaterialname
            // 
            tbx_addmaterialname.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tbx_addmaterialname.Location = new Point(105, 39);
            tbx_addmaterialname.Margin = new Padding(4, 5, 4, 5);
            tbx_addmaterialname.MinimumSize = new Size(1, 16);
            tbx_addmaterialname.Name = "tbx_addmaterialname";
            tbx_addmaterialname.Padding = new Padding(5);
            tbx_addmaterialname.ShowText = false;
            tbx_addmaterialname.Size = new Size(121, 29);
            tbx_addmaterialname.TabIndex = 7;
            tbx_addmaterialname.Text = "ABC";
            tbx_addmaterialname.TextAlignment = ContentAlignment.MiddleCenter;
            tbx_addmaterialname.Watermark = "";
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(12, 45);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(86, 23);
            uiLabel2.TabIndex = 6;
            uiLabel2.Text = "物料名称:";
            uiLabel2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // uiGroupBox1
            // 
            uiGroupBox1.Controls.Add(but_Query);
            uiGroupBox1.Controls.Add(uiComboBox1);
            uiGroupBox1.Controls.Add(uiLabel1);
            uiGroupBox1.Controls.Add(txt_AxisX);
            uiGroupBox1.Controls.Add(uiLabel18);
            uiGroupBox1.Dock = DockStyle.Top;
            uiGroupBox1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiGroupBox1.Location = new Point(0, 0);
            uiGroupBox1.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox1.MinimumSize = new Size(1, 1);
            uiGroupBox1.Name = "uiGroupBox1";
            uiGroupBox1.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox1.Size = new Size(853, 92);
            uiGroupBox1.TabIndex = 0;
            uiGroupBox1.Text = "查询";
            uiGroupBox1.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // but_Query
            // 
            but_Query.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_Query.Location = new Point(450, 37);
            but_Query.MinimumSize = new Size(1, 1);
            but_Query.Name = "but_Query";
            but_Query.Size = new Size(90, 29);
            but_Query.TabIndex = 27;
            but_Query.Text = "查询";
            but_Query.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            but_Query.Click += but_Query_Click;
            // 
            // uiComboBox1
            // 
            uiComboBox1.DataSource = null;
            uiComboBox1.FillColor = Color.White;
            uiComboBox1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiComboBox1.ItemHoverColor = Color.FromArgb(155, 200, 255);
            uiComboBox1.Items.AddRange(new object[] { "生效", "失效" });
            uiComboBox1.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            uiComboBox1.Location = new Point(312, 37);
            uiComboBox1.Margin = new Padding(4, 5, 4, 5);
            uiComboBox1.MinimumSize = new Size(63, 0);
            uiComboBox1.Name = "uiComboBox1";
            uiComboBox1.Padding = new Padding(0, 0, 30, 2);
            uiComboBox1.Size = new Size(126, 29);
            uiComboBox1.SymbolSize = 24;
            uiComboBox1.TabIndex = 26;
            uiComboBox1.TextAlignment = ContentAlignment.MiddleCenter;
            uiComboBox1.Watermark = "";
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(233, 43);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(72, 23);
            uiLabel1.TabIndex = 6;
            uiLabel1.Text = "生失效:";
            uiLabel1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txt_AxisX
            // 
            txt_AxisX.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txt_AxisX.Location = new Point(105, 37);
            txt_AxisX.Margin = new Padding(4, 5, 4, 5);
            txt_AxisX.MinimumSize = new Size(1, 16);
            txt_AxisX.Name = "txt_AxisX";
            txt_AxisX.Padding = new Padding(5);
            txt_AxisX.ShowText = false;
            txt_AxisX.Size = new Size(121, 29);
            txt_AxisX.TabIndex = 5;
            txt_AxisX.Text = "ABC";
            txt_AxisX.TextAlignment = ContentAlignment.MiddleCenter;
            txt_AxisX.Watermark = "";
            // 
            // uiLabel18
            // 
            uiLabel18.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel18.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel18.Location = new Point(12, 43);
            uiLabel18.Name = "uiLabel18";
            uiLabel18.Size = new Size(86, 23);
            uiLabel18.TabIndex = 4;
            uiLabel18.Text = "物料名称:";
            uiLabel18.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ProcessFlowFrm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(853, 629);
            Controls.Add(uiPanel1);
            Name = "ProcessFlowFrm";
            Text = "ProcessFlowFrm";
            uiPanel1.ResumeLayout(false);
            uiGroupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView3).EndInit();
            uiGroupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            uiGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            uiGroupBox2.ResumeLayout(false);
            uiGroupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UIGroupBox uiGroupBox2;
        private Sunny.UI.UIGroupBox uiGroupBox3;
        private Sunny.UI.UIGroupBox uiGroupBox4;
        private Sunny.UI.UIGroupBox uiGroupBox5;
        private DataGridView dataGridView2;
        private DataGridView dataGridView1;
        private DataGridView dataGridView3;
        private Sunny.UI.UITextBox txt_AxisX;
        private Sunny.UI.UILabel uiLabel18;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIComboBox uiComboBox1;
        private Sunny.UI.UIButton but_Query;
        private Sunny.UI.UITextBox tbx_addmaterialname;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UITextBox tbx_addprocessname;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UIComboBox cbx_addsite;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UIButton but_addprocess;
        private Sunny.UI.UIButton but_deleteprocess;
        private Sunny.UI.UIButton but_effectlose;
        private Sunny.UI.UIButton but_deleteitem;
        private Sunny.UI.UIButton but_addprocessitem;
        private Sunny.UI.UIButton but_insert;
        private Sunny.UI.UITextBox tbx_Qtime;
        private Sunny.UI.UILabel uiLabel4;
    }
}