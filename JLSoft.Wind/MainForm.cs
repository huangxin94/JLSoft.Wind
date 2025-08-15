using System.Data;
using System.Windows.Forms;
using DuckDB.NET.Data;
using JLSoft.Wind.Adapter;
using JLSoft.Wind.Class;
using JLSoft.Wind.CustomControl;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Database.Struct;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;
using JLSoft.Wind.Services.Connect;
using JLSoft.Wind.Services.DuckDb;
using JLSoft.Wind.Services.Status;
using JLSoft.Wind.Settings;
using JLSoft.Wind.UserControl;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using OpenTK.Audio.OpenAL;
using Sunny.UI;
using Sunny.UI.Win32;
using Timer = System.Windows.Forms.Timer;

namespace JLSoft.Wind
{
    public partial class MainForm : Form
    {
        // ������������ɵĿؼ�
        private List<IndustrialModule> modules = new List<IndustrialModule>();// ��ҵģ���б�
        private float rotationAngle = 0; // ��¼��ǰ��ת�Ƕ�

        private readonly DuckDbService _dbService;// ���ݿ����ʵ��
        private UIRichTextBox logBox;// ��־��

        // �� MainForm ����������³�Ա����
        private bool _isUserLoggedIn = false;/// �Ƿ��ѵ�¼ 
        private TimechartService _timechartService;//
        CommunicationCoordinator _coordinator; //ͨ��Э�������PLC����
        private DeviceMonitor _deviceMonitor; // �豸��ع�����
        private PlcConnection _plcConn;
        public static List<PositionInfo> AllPositions { get; private set; }

        public MainForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            try
            {
                // ��ʼ������
                _dbService = new DuckDbService(Program.DuckDBConnectionString);
            }
            catch (TypeInitializationException ex)
            {
                // ��ӡ�ڲ��쳣����
                Console.WriteLine($"Inner exception: {ex.InnerException}");
                throw;
            }
            var timer = new Timer { Interval = 2000 };
            timer.Tick += (s, e) => ToggleState();
            timer.Start();
            LoadImageFromFolder();
            InitializeDataGridView();
            dataGridView1.CellDoubleClick += DataGridView1_CellClick;

            factoryLayoutControl1.ControlClick += OnControlClicked;
            factoryLayoutControl1.ControlColorChanged += OnColorChanged;


            dataGridView2.CellDoubleClick += DataGridView1_CellClick;

            factoryLayoutControl2.ControlClick += OnControlClicked;
            factoryLayoutControl2.ControlColorChanged += OnColorChanged;


            tabControl1.Selecting += TabControl1_Selecting;

            ��ǰ��¼�û�ToolStripMenuItem.Text = "��ǰ��¼�û���δ��¼"; // ���ò˵����ı�

        }

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == tabPage2)
            {
                if (!_isUserLoggedIn)
                {
                    // �û�δ��¼����ʾ��¼
                    LoginFrm loginForm = new LoginFrm();
                    loginForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
                    DialogResult result = loginForm.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        // ��¼�ɹ�����ȡ�û���ɫ
                        _isUserLoggedIn = true;
                        ��ǰ��¼�û�ToolStripMenuItem.Text = $"��ǰ��¼�û���{SessionManager.CurrentUser.Username} (ְ��: {SessionManager.CurrentUser.Grade})"; // ���²˵����ı�
                        if (SessionManager.CurrentUser.Grade != "����ʦ" && SessionManager.CurrentUser.Grade != "��������Ա")
                        {
                            // �û���ɫȨ�޲��㣬��ʾ��Ϣ��ȡ��ѡ��
                            MessageBox.Show("��Ȩ�޷��ʸ�ҳ��", "Ȩ�޲���", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        // ��¼ȡ����ȡ��ѡ��
                        e.Cancel = true;
                    }
                }
                else
                {
                    if (SessionManager.CurrentUser.Grade != "����ʦ" && SessionManager.CurrentUser.Grade != "��������Ա")
                    {
                        // �û���ɫȨ�޲��㣬��ʾ��Ϣ��ȡ��ѡ��
                        MessageBox.Show("��Ȩ�޷��ʸ�ҳ��", "Ȩ�޲���", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }

                Application.DoEvents();
                factoryLayoutControl1.ForceRescale();
            }
        }

        /// <summary>
        /// ��ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // �ж��Ƿ�����Ч���е�����ų���ͷ����������
            if (e.RowIndex >= 0)
            {

                string column0Data = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();
                // ��ȡ��3��4��5��6�е�����

                // ʵ������ҳ�棨������ҳ��Ĺ��캯���ܽ�����Щ���ݣ�
                ActualProcessItemFrm sonForm = new ActualProcessItemFrm(_dbService, column0Data);
                sonForm.Text = "��Ʒ������������";  // ���ô������
                sonForm.StartPosition = FormStartPosition.CenterScreen;
                sonForm.ShowDialog();
            }
        }
        private DataTable dataTable; // ����Ϊ���Ա�Ա����
        private void InitializeDataGridView()
        {
            try
            {

                // ����SQL�Ͳ���
                string sql = @"select
	                            a.mainid as ����,
	                            a.Status as ״̬,
	                            a.CurrentSite as ��ǰվ��,
	                            a.Progress as ��ɽ���,
	                            a.type as ����,
	                            a.ProductName as ��Ʒ����,
	                            a.ProductCode as ��Ʒ����,
	                            a.StartTime as ��ʼʱ��,
	                            a.EndTime as ���ʱ��,
	                            b.Qtime as �趨Qtimeֵ,
	                            b.StartQtime as ��ʼ����Qtimeʱ��,
	                            b.ActualQtime as ʵ��Qtime
                            from
	                            ProductiveProcessMain a
                            left join actualprocessitem b
                                on
	                            a.actualprocessitemid = b.actualprocessitemid
                            where
	                            a.Status = ?";
                var parameters = new List<DuckDB.NET.Data.DuckDBParameter>();
                parameters.Add(new DuckDB.NET.Data.DuckDBParameter { Value = "����" });

                // 3. ��ѯ���ݿ�
                dataTable = _dbService.ExecuteQuery(sql, parameters.ToArray());

            }
            catch (Exception ex)
            {
                LogManager.Log($"���������ݲ�ѯʧ��: {ex.Message}");
            }

            dataGridView1.DataSource = dataTable; // ֱ�Ӱ�����Դ�������ֶ�����


            dataGridView2.DataSource = dataTable; // ֱ�Ӱ�����Դ�������ֶ�����
            if (dataTable != null)
            {
                dataGridView1.Columns["����"].Visible = false;
                dataGridView2.Columns["����"].Visible = false;
            }
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private readonly WaferRobotVisualizer robot = new WaferRobotVisualizer();
        private bool _isWorking;
        private void ToggleState()
        {
            _isWorking = !_isWorking;
            robot.UpdateState(
                position: _isWorking ? 0.8f : 0.2f,
                zHeight: _isWorking ? 0.7f : 0.3f,
                isGripping: _isWorking
            );
        }
        int imagename = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {

        }


        private void LoadImageFromFolder()
        {

        }

        // ��תͼƬ��˳ʱ��90�ȣ�
        private void RotateImage()
        {

        }
        private const int MoveStep = 10; // ÿ���ƶ��Ĳ���
        private void button1_Click(object sender, EventArgs e)
        {
            RotateImage();
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            MovePictureBox(-MoveStep);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            MovePictureBox(MoveStep);
        }


        private void MovePictureBox(int step)
        {

        }

        private void uiDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void �豸��λToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            RobotSettingFrm settingFrm = new RobotSettingFrm();
            settingFrm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            settingFrm.ShowDialog(this);

        }

        private void ��ǰ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            ProcessFlowFrm sonForm = new ProcessFlowFrm(_dbService);
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.ShowDialog(this);
        }

        private void alignerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "aligner";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ��aligner�鿴/����ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("aligner�����", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void alignerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "�Ƕ�̨";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ���Ƕ�̨�鿴/����ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("�Ƕ�̨�����", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void lDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "LD";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ��LD�鿴/����ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("LD�����", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void p1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "P1";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ��P1�鿴/����ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("P1�����", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void x1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "X1";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ��X1�鿴/����ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("X1�����", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void Ѱ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "Ѱ����";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ��Ѱ�����鿴/����ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("Ѱ���������", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void ����LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "����Log";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ������Log�鿴ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("����Log�����", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void �ղ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ʵ�������е���ҳ�洰��
            SonForm sonForm = new SonForm();

            // ���ô������ԣ���ѡ��
            sonForm.Text = "�ղ���";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            sonForm.label1.Text = "����һ���ղ��ܲ鿴ҳ��"; // ���ñ�ǩ�ı�
            // ��ʾ���壨���ַ�ʽ��ѡ��

            // ��ʽ1����ģ̬�Ի�����ʽ��ʾ���û������ȹر��Ӵ�����ܲ��������壩
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // ����"ȷ��"����������Ӵ�����ȷ�ϰ�ť��
                MessageBox.Show("�ղ��������", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // ��ʽ2���Է�ģ̬��ʽ��ʾ���û���ͬʱ������������Ӵ��壩
            // sonForm.Show(this);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MovePictureBox(-MoveStep);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MovePictureBox(MoveStep);
        }
        private void OnControlClicked(object sender, FactoryLayoutControl.ControlOperationEventArgs e)
        {
            if (e.TargetControl != null)
            {
                // �Զ��崦���߼�
                if (e.TargetControl is DeviceBlock block)
                {
                    string deviceCode = block.DeviceCode;
                    DeviceStateFrm devicestateFrm = new DeviceStateFrm(deviceCode);
                    devicestateFrm.Text = deviceCode;  // ���ô������
                    devicestateFrm.textBox1.Text = deviceCode; // �����豸����
                    devicestateFrm.StartPosition = FormStartPosition.CenterParent;
                    devicestateFrm.ShowDialog(this);
                }
            }

            if (!string.IsNullOrEmpty(e.ErrorMessage))
            {
                MessageBox.Show($"��������: {e.ErrorMessage}");
            }
        }

        private void OnColorChanged(object sender, FactoryLayoutControl.ControlOperationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ErrorMessage))
            {
                MessageBox.Show($"��ɫ����: {e.ErrorMessage}");
            }
        }
        /// <summary>
        /// ���빤��ʦ����ҳ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void �û�����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManagementUsersFrm management = new ManagementUsersFrm(_dbService);
            management.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
            DialogResult result = management.ShowDialog(this);
        }
        /// <summary>
        /// �˳���¼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void �˳���¼ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SessionManager.CurrentUser == null)
            {
                MessageBox.Show("��ǰû�е�¼�û���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult result = MessageBox.Show("�Ƿ�ȷ���˳���¼״̬��", "�˳�", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                ��ǰ��¼�û�ToolStripMenuItem.Text = "��ǰ��¼�û���δ��¼"; // ���²˵����ı�
                _isUserLoggedIn = false;
                SessionManager.Logout();// �����ǰ�û��Ự
            }
        }

        private void ��ǰ��¼�û�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SessionManager.CurrentUser == null)
            {

                LoginFrm loginForm = new LoginFrm();
                loginForm.StartPosition = FormStartPosition.CenterParent;  // ������ʾ
                DialogResult result = loginForm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // ��¼�ɹ�����ȡ�û���ɫ
                    _isUserLoggedIn = true;
                    ��ǰ��¼�û�ToolStripMenuItem.Text = $"��ǰ��¼�û���{SessionManager.CurrentUser.Username} (ְ��: {SessionManager.CurrentUser.Grade})"; // ���²˵����ı�
                }
            }

        }
        public MitsubishiPLC plc;
        /// <summary>
        /// �ϵ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_uppower_Click(object sender, EventArgs e)
        {
            but_uppower.Enabled = false;
            but_uppower.Text = "������...";
            try
            {
                plc = MitsubishiPLC.Instance;
                var result = await plc.ConnectAsync();

                if (result.IsSuccess)
                {
                    but_uppower.Text = "������";
                    but_uppower.FillColor = Color.Green;
                    LogManager.Log("PLC���ӳɹ�",LogLevel.Info, "PLC.Main");
                }
                else
                {
                    but_uppower.Text = "�ϵ�";
                    LogManager.Log($"PLC����ʧ��: {result.Message}",LogLevel.Info, "PLC.Main");
                }
            }
            catch (Exception ex)
            {
                but_uppower.Text = "�ϵ�";
                LogManager.Log($"�����쳣: {ex.Message}");
            }
            finally
            {
                but_uppower.Enabled = true;
            }
            //MessageBox.Show(uiRichTextBox1.Text.ToString(), "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private SequenceTaskExecutor _executor;
        private CancellationTokenSource _cts;
        /// <summary>
        /// д��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_readin_Click(object sender, EventArgs e)
        {

            _cts = new CancellationTokenSource();
            int deviceIndex = (int)CbxFacility.SelectedValue; // U1Ϊ0��U2Ϊ1����������
            string deviceCode = (string)CbxFacility.SelectedValue; // ����ӽ���ѡ��
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                return;
            }
            var plcConn = _coordinator.GetPlcConnection(1); // 1Ϊ��PLCվ��

            if (plcConn == null)
            {
                MessageBox.Show("PLC����δ��ʼ����");
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, "", new Positions(), _deviceMonitor);

            try
            {
                await _executor.PutRunAsync(_cts.Token);
                LogManager.Log($"ʱ������{deviceIndex}��ɣ�");
            }
            catch (OperationCanceledException)
            {
                LogManager.Log($"ʱ������{deviceIndex}��ȡ����", LogLevel.Warn);
            }
            catch (Exception ex)
            {
                LogManager.Log($"ʱ������{deviceIndex}�쳣: {ex.Message}", LogLevel.Error);
            }
            /*
            try
            {
                but_readin.Enabled = false;
                plc = _coordinator.GetPlcConnection(1).Plc;
                if (plc == null) plc = MitsubishiPLC.Instance;

                if (!plc.IsConnected)
                {
                    LogManager.Log("PLCδ���ӣ����ڳ�������...");
                    await plc.ConnectAsync();
                }

                var writeResult = await plc.WriteDataAsync("B100", 0);
                if (!writeResult.IsSuccess)
                    LogManager.Log($"д��ʧ��: {writeResult.Message}");

                short[] values = { 0, 0, 0 };
                var batchResult = await plc.WriteDataAsync("B200", values);

                LogManager.Log(batchResult.IsSuccess ?
                    "����д��ɹ�" : $"����д��ʧ��: {batchResult.Message}");
            }
            catch (Exception ex)
            {
                LogManager.Log($"д������쳣: {ex.Message}");
            }
            finally
            {
                but_readin.Enabled = true;
            }
            */
        }
        /// <summary>
        /// ��ȡ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_readout_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // 1. ����ڵ��б�
            var nodes = new List<NetworkNode>
            {
                new NetworkNode
                {
                    StationId = 1,
                    IpAddress = ConfigService.GetPlcIp(),
                    Port = ConfigService.GetPlcPort(),
                    Name = "��PLC"
                }
            };
            AllPositions = ConfigService.GetDevicePositions(null);
            // 2. ����Э�������Զ��������ӣ�
            _coordinator = new CommunicationCoordinator(nodes);

            // 3. ���ݸ�TimechartService
            _timechartService = new TimechartService(_coordinator);

            _plcConn = _coordinator.GetPlcConnection(1);
            _deviceMonitor = new DeviceMonitor(_plcConn);
            _deviceMonitor.DeviceStatesUpdated += OnDeviceStatesUpdated;

            // 4. �����豸���
            await RobotManager.Instance.InitializeAsync();
            if (RobotManager.Instance.IsConnected)
                LogManager.Log("������������", LogLevel.Info, "Robot.Main");
            else
                LogManager.Log("������δ����", LogLevel.Warn, "Robot.Main");

            ///// 5. ��ʼ���豸״̬���
            var deviceIndices = ConfigService.GetDeviceStations();
            var deviceCodes = deviceIndices.ToList();
            CbxFacility.DataSource = deviceCodes;
            CbxFacility.DisplayMember = "DeviceCode";   // ��ʾ�豸���
            CbxFacility.ValueMember = "Index";   // ѡ��ʱȡ�豸����


            logBox = uiRichTextBox1;
            //tabControl1.SelectedIndex = 1;
            LogManager.Initialize(logBox);
            LogManager.Log("��־ϵͳ��ʼ�����", LogLevel.Info);
            uiButton1.BringToFront();


        }

        private void OnDeviceStatesUpdated(Dictionary<string, DeviceMonitor.DeviceState> deviceStates)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnDeviceStatesUpdated(deviceStates));
                return;
            }

            // ���������豸UI
            foreach (var (deviceCode, state) in deviceStates)
            {
                UpdateDeviceUI(deviceCode, state);
            }
        }

        private void UpdateDeviceUI(string deviceCode, DeviceMonitor.DeviceState state)
        {
            /* ��ʱȡ��
            Color color = Color.Gray;

            if (state.Running) color = Color.Green;
            else if (state.Idle) color = Color.Yellow;
            else if (state.Paused) color = Color.Orange;
            else if (state.Fault) color = Color.Red;

            factoryLayoutControl1.SetDeviceBlockColor(deviceCode, color);

            // ��ʾ����״̬
            factoryLayoutControl2.SetDeviceBlockColor(deviceCode, color);
            */

            DeviceStatus deviceStatus = DeviceStatus.Idle;

            if (state.Running) deviceStatus = DeviceStatus.Running;
            else if (state.Idle) deviceStatus = DeviceStatus.Idle;
            else if (state.Paused) deviceStatus = DeviceStatus.Paused;
            else if (state.Fault) deviceStatus = DeviceStatus.Fault;
            else deviceStatus = DeviceStatus.Offline;

            factoryLayoutControl1.SetDeviceBlockBorderColor(deviceCode, deviceStatus);

            // ��ʾ����״̬
            factoryLayoutControl2.SetDeviceBlockBorderColor(deviceCode, deviceStatus);

            uiLight1.State = (RobotManager.Instance.IsConnected ? UILightState.On : UILightState.Off);
            uiLight2.State = (_plcConn != null && _plcConn.IsConnected) ? UILightState.On : UILightState.Off;

        }
        /// <summary>
        /// �����־
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label2_Click(object sender, EventArgs e)
        {
            this.uiRichTextBox1.Text = string.Empty; // �����־
        }

        private void but_stop_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// �����Ƭ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_orientation_up_Click(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            var slot = cbx_Slot.Text;
            int deviceIndex = 0; // U1Ϊ0��U2Ϊ1����������
            if (CbxFacility.Text == "")
            {
                MessageBox.Show("����ѡ���豸��ţ�");
                return;
            }
            string deviceCode = CbxFacility.Text; // ����ӽ���ѡ��
            var coord = new Positions();
            if (string.IsNullOrEmpty(slot))
            {
                coord = ConfigService.FindPosition(deviceCode);
            }
            else
            {
                coord = ConfigService.FindPosition(deviceCode, slot);
            }
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                return;
            }
            var plcConn = _coordinator.GetPlcConnection(1); // 1Ϊ��PLCվ��

            if (plcConn == null)
            {
                MessageBox.Show("PLC����δ��ʼ����");
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor);

            try
            {
                await _executor.PutRunAsync(_cts.Token);
                LogManager.Log($"ʱ������{deviceIndex}��ɣ�");
            }
            catch (OperationCanceledException)
            {
                LogManager.Log($"ʱ������{deviceIndex}��ȡ����", LogLevel.Warn);
            }
            catch (Exception ex)
            {
                LogManager.Log($"ʱ������{deviceIndex}�쳣: {ex.Message}", LogLevel.Error);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_orientation_dow_Click(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            int deviceIndex = 0; // U1Ϊ0��U2Ϊ1����������
            var slot = cbx_Slot.Text;
            if (CbxFacility.Text == "")
            {
                MessageBox.Show("����ѡ���豸��ţ�");
                return;
            }
            string deviceCode = CbxFacility.Text; // ����ӽ���ѡ��
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                return;
            }
            var plcConn = _coordinator.GetPlcConnection(1); // 1Ϊ��PLCվ��
            var coord = new Positions();
            if (string.IsNullOrEmpty(slot))
            {
                coord = ConfigService.FindPosition(deviceCode);
            }
            else
            {
                coord = ConfigService.FindPosition(deviceCode, slot);
            }
            if (plcConn == null)
            {
                MessageBox.Show("PLC����δ��ʼ����");
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor);

            try
            {
                await _executor.GetRunAsync(_cts.Token);
                LogManager.Log($"ʱ������{deviceIndex}��ɣ�");
            }
            catch (OperationCanceledException)
            {
                LogManager.Log($"ʱ������{deviceIndex}��ȡ����", LogLevel.Warn);
            }
            catch (Exception ex)
            {
                LogManager.Log($"ʱ������{deviceIndex}�쳣: {ex.Message}", LogLevel.Error);
            }
        }

        private void CbxFacility_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CbxFacility.SelectedItem is DeviceIndices selectedDevice)
            {
                if (selectedDevice.SubStations != null && selectedDevice.SubStations.Any())
                {
                    // ����վ
                    cbx_Slot.DataSource = selectedDevice.SubStations;
                    cbx_Slot.DisplayMember = "Name";  // ��ʾ��վ����
                    cbx_Slot.ValueMember = "Name";    // ֵȡ��վ����
                    cbx_Slot.Enabled = true;
                }
                else
                {
                    // û����վ�����ComboBox������
                    cbx_Slot.DataSource = null; // �������Դ
                    cbx_Slot.Items.Clear();     // ���������
                    cbx_Slot.Text = string.Empty; // �����ʾ�ı�
                    cbx_Slot.Enabled = false;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void �豸�źŲ鿴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EquipmentStatusFrm sonForm = new EquipmentStatusFrm(_deviceMonitor);
            sonForm.Text = "�豸�źŲ鿴";  // ���ô������
            sonForm.StartPosition = FormStartPosition.CenterScreen;
            sonForm.ShowDialog(this);
        }

        private void ����ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
    }
}
