using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using DMCE3000;
using DuckDB.NET.Data;
using JLSoft.Wind.Adapter;
using JLSoft.Wind.Class;
using JLSoft.Wind.CustomControl;
using JLSoft.Wind.Database;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Database.Struct;
using JLSoft.Wind.Enum;
using JLSoft.Wind.IServices;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;
using JLSoft.Wind.Services.Connect;
using JLSoft.Wind.Services.DuckDb;
using JLSoft.Wind.Services.Status;
using JLSoft.Wind.Settings;
using JLSoft.Wind.UIHelpers;
using JLSoft.Wind.UserControl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using OpenTK.Audio.OpenAL;
using Sunny.UI;
using Sunny.UI.Win32;
using static JLSoft.Wind.Database.StationName;
using static JLSoft.Wind.Services.Leisai_Axis;
using DeviceStatus = JLSoft.Wind.UserControl.DeviceStatus;
using Timer = System.Windows.Forms.Timer;

namespace JLSoft.Wind
{
    public partial class MainForm : Form
    {
        // ������������ɵĿؼ�
        private float rotationAngle = 0; // ��¼��ǰ��ת�Ƕ�

        public static string AxisConfigFilePath = Application.StartupPath + "\\Axis_Config.ini";// �˶����ƿ������ļ�·��
        public static MainForm _instance;



        private readonly DuckDbService _dbService;// ���ݿ����ʵ��

        // �� MainForm ����������³�Ա����
        private bool _isUserLoggedIn = false;/// �Ƿ��ѵ�¼ 
        CommunicationCoordinator _coordinator; //ͨ��Э�������PLC����
        private DeviceMonitor _deviceMonitor; // �豸��ع�����
        private PlcConnection _plcConn;

        private static string _currentWaferSize = "8Ӣ��"; // Ĭ��8Ӣ��
        private static string _currentWaferType = "Wafer";


        public Timer _inputRefreshTimer;

        public Timer _outputRefreshTimer;
        public Timer _positionRefreshTimer;

        public static string _currentStationPos;


        public static string CurrentWaferSize
        {
            get => _currentWaferSize;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _currentWaferSize = value;
            }
        }

        public static string CurrentWaferType
        {
            get => _currentWaferType;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _currentWaferType = value;
            }
        }
        public static List<PositionInfo> AllPositions { get; private set; }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            EventAggregator.LoadPortColorUpdateRequested -= UpdateLoadPortColor;
            base.OnFormClosed(e);
        }
        public MainForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            _instance = this;
            EventAggregator.LoadPortColorUpdateRequested += UpdateLoadPortColor;

            // 4. ��־ϵͳ��ʼ��
            LogManager.Initialize(uiRichTextBox1);
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

            // 2. ��ʼ���豸������
            var deviceIndices = ConfigService.GetDeviceStations();
            CbxFacility.DataSource = deviceIndices;
            CbxFacility.DisplayMember = "DeviceCode";
            CbxFacility.ValueMember = "Index";




        }
        public void UpdateLoadPortColor(string loadPortName, string mappingString)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, string>(UpdateLoadPortColor), loadPortName, mappingString);
                return;
            }

            switch (loadPortName)
            {
                case "LoadPort1":
                    loadPort21.UpdateMapping(mappingString); // ���� LoadPort2 �ؼ��� SetColor ����
                    break;
                case "LoadPort2":
                    loadPort22.UpdateMapping(mappingString);
                    break;
            }
        }
        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == tabPage2)
            {
                /*
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
                */

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
                CustomMessageBox.Show("P1�����", "�ɹ�", MessageType.Warning);
            }
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
            bool initSuccess = await _initializer.InitializeAsync();
            if (!initSuccess)
            {
                uiLight1.State = UILightState.Off;
                uiLight2.State = UILightState.Off;
                LogManager.Log("���ӳ�ʼ��ʧ��", LogLevel.Error);
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
            /*
            _cts = new CancellationTokenSource();
            int deviceIndex = (int)CbxFacility.SelectedValue; // U1Ϊ0��U2Ϊ1����������
            string deviceCode = (string)CbxFacility.SelectedValue; // ����ӽ���ѡ��
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                return;
            }
            var plcConn = _coordinator?.GetPlcConnection(1); // 1Ϊ��PLCվ��

            if (plcConn == null)
            {
                MessageBox.Show("PLC����δ��ʼ����", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, "", new Positions(), _deviceMonitor, _dbService);

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
            */

            await WaferSelect();
        }
        /// <summary>
        /// ��ȡ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_readout_Click(object sender, EventArgs e)
        {
            //_cts?.Cancel();
        }
        private readonly SystemInitializer _initializer = new SystemInitializer();
        private async void MainForm_Load(object sender, EventArgs e)
        {
            // ���ĳ�ʼ������¼�
            _initializer.InitializationCompleted += OnSystemInitialized;

            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            // ��ʼ��ʼ��
                            bool initSuccess = await _initializer.InitializeAsync();
                            if (!initSuccess)
                            {
                                uiLight1.State = UILightState.Off;
                                uiLight2.State = UILightState.Off;
                                LogManager.Log("���ӳ�ʼ��ʧ��", LogLevel.Error);
                            }
                            if (LeisaiIO.leisaiIO_Init())
                            {
                                LedControl.LedFirst();
                                StartIOInRefresh(50);
                                StartIOOutRefresh(100);
                                StartAxisPosition(10);
                            }
                            return true; // ��Ҫ��������ֵ
                        },
                        "ϵͳ��ʼ����\n���Ժ�..."
                    );
        }

        private void StartIOInRefresh(int v)
        {
            // ȷ����ʱ������
            if (_inputRefreshTimer == null)
            {
                _inputRefreshTimer = new Timer();
                _inputRefreshTimer.Tick += InputRefreshTimer_Tick;
            }

            // ���ö�ʱ������
            _inputRefreshTimer.Interval = v;

            // ������ʱ��
            _inputRefreshTimer.Start();
        }
        private void InputRefreshTimer_Tick(object sender, EventArgs e)
        {
            LeisaiIO.ReadInputState();
        }

        private void StartIOOutRefresh(int v)
        {
            // ȷ����ʱ������
            if (_outputRefreshTimer == null)
            {
                _outputRefreshTimer = new Timer();
                _outputRefreshTimer.Tick += OutputRefreshTimer_Tick;
            }

            // ���ö�ʱ������
            _outputRefreshTimer.Interval = v;

            // ������ʱ��
            _outputRefreshTimer.Start();
        }

        private void OutputRefreshTimer_Tick(object sender, EventArgs e)
        {
            LeisaiIO.ReadOutputState();
        }

        /// <summary>
        /// ��ȡ��λ��timer
        /// </summary>
        /// <param name="v"></param>
        private void StartAxisPosition(int v)
        {
            if (_positionRefreshTimer == null)
            {
                _positionRefreshTimer = new Timer();
                _positionRefreshTimer.Tick += PositionRefreshTimer_Tick;
            }

            // ���ö�ʱ������
            _positionRefreshTimer.Interval = v;

            // ������ʱ��
            _positionRefreshTimer.Start();
        }

        /// <summary>
        /// ��ȡλ�ò���ʾUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PositionRefreshTimer_Tick(object sender, EventArgs e)
        {
            ushort axisXState = '0';
            ushort axisYState = '0';
            ushort axisZState = '0';
            Leisai_Axis.EtherCat_Status = Leisai_Axis.Leisai_Geterrcode();
            Leisai_Axis.X_Axis_Status = Leisai_Axis.Leisai_GetAxisStatus((ushort)AxisName.X, ref axisXState);
            Leisai_Axis.Y_Axis_Status = Leisai_Axis.Leisai_GetAxisStatus((ushort)AxisName.Y, ref axisYState);
            Leisai_Axis.Z_Axis_Status = Leisai_Axis.Leisai_GetAxisStatus((ushort)AxisName.Z, ref axisZState);



            Axis_INP.X_pos = Leisai_Axis.Leisai_GetEncoder((ushort)AxisName.X);
            Axis_INP.Y_pos = Leisai_Axis.Leisai_GetEncoder((ushort)AxisName.Y);
            Axis_INP.Z_pos = Leisai_Axis.Leisai_GetEncoder((ushort)AxisName.Z);
            bool xReady, yReady, zReady;
            if (Leisai_Axis.Leisai_CheckDone((ushort)AxisName.X) == 0)
            {
                TeachServer.Axis_X_TServer_State = true;
            }
            else
            {
                xReady = true;

                TeachServer.Axis_X_TServer_State = false;
            }
            if (Leisai_Axis.Leisai_CheckDone((ushort)AxisName.Y) == 0)
            {
                yReady = false;

                TeachServer.Axis_Y_TServer_State = true;
            }
            else
            {
                yReady = true;
                TeachServer.Axis_Y_TServer_State = false;
            }
            if (Leisai_Axis.Leisai_CheckDone((ushort)AxisName.Z) == 0)
            {
                zReady = false;
                TeachServer.Axis_Z_TServer_State = true;
            }
            else
            {
                zReady = true;
                TeachServer.Axis_Z_TServer_State = false;
            }
        }
        private void OnSystemInitialized()
        {
            // 1. ����UI״̬
            uiLight1.State = _initializer.IsRobotConnected
                ? UILightState.On : UILightState.Off;

            uiLight2.State = _initializer.IsPlcConnected
                ? UILightState.On : UILightState.Off;



            // 3. ���豸����¼�
            _deviceMonitor = _initializer.GetDeviceMonitor();
            _plcConn = _initializer.GetPlcConnection();
            if (_deviceMonitor != null)
            {
                _deviceMonitor.DeviceStatesUpdated += OnDeviceStatesUpdated;

                // ��������һ��״̬����
                var currentStates = _deviceMonitor.CurrentDeviceStates;
                if (currentStates != null)
                {
                    OnDeviceStatesUpdated(new Dictionary<string, DeviceMonitor.DeviceState>(currentStates));
                }
                else
                {
                    // �����ǰ״̬Ϊ�գ��ֶ����������豸Ϊ����
                    SetAllDevicesToOfflineUI();
                }
            }
            else
            {
                // �豸���Ϊ��ʱ�����������豸Ϊ����
                SetAllDevicesToOfflineUI();
            }

            LogManager.Log("ϵͳ��ʼ�����", LogLevel.Info);
            uiButton1.BringToFront();
        }

        // �������������������豸UIΪ����״̬
        private void SetAllDevicesToOfflineUI()
        {
            try
            {
                var allDeviceCodes = new[] { "V1", "U1", "S3", "M1", "S4", "C1", "R1", "G1",
                                   "A1", "A2", "A3", "A4", "Ѱ�߻�", "�Ƕ�̨", "LP1", "LP2" };

                foreach (var deviceCode in allDeviceCodes)
                {
                    factoryLayoutControl1.SetDeviceBlockBorderColor(deviceCode, DeviceStatus.Offline);
                    factoryLayoutControl2.SetDeviceBlockBorderColor(deviceCode, DeviceStatus.Offline);
                }

                // ��������ָʾ��
                uiLight2.State = UILightState.Off;
            }
            catch (Exception ex)
            {
                LogManager.Log($"�����豸����UIʧ��: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnDeviceStatesUpdated(Dictionary<string, DeviceMonitor.DeviceState> deviceStates)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnDeviceStatesUpdated(deviceStates));
                return;
            }

            // ��ȡDeviceMonitor������ģʽ��Ϣ
            bool isOfflineMode = _deviceMonitor?.IsOfflineMode ?? true;
            bool isPlcConnected = _deviceMonitor?.IsPlcConnected ?? false;

            // ��������ģʽ�����豸UI
            if (!isPlcConnected || isOfflineMode)
            {
                SetAllDevicesToOfflineUI();
                return;
            }

            // ���������豸״̬
            if (deviceStates != null)
            {
                foreach (var (deviceCode, state) in deviceStates)
                {
                    UpdateDeviceUI(deviceCode, state);
                }
            }

            // ��������ָʾ��
            UpdateConnectionIndicators(isPlcConnected, isOfflineMode);
        }


        // ������������������ָʾ��
        private void UpdateConnectionIndicators(bool isPlcConnected, bool isOfflineMode)
        {
            if (!isPlcConnected)
            {
                uiLight2.State = UILightState.Off; // PLC���ӵ�Ϩ��
            }
            else if (isOfflineMode)
            {
                uiLight2.State = UILightState.Blink; // PLC���ӵ���������ģʽ����˸
            }
            else
            {
                uiLight2.State = UILightState.On; // PLC��������
            }
        }
        private void UpdateDeviceUI(string deviceCode, DeviceMonitor.DeviceState state)
        {

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
        /// <summary>
        /// ֹͣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_stop_Click(object sender, EventArgs e)
        {
            Leisai_Axis.Leisai_Stop(0, 1);
            Leisai_Axis.Leisai_Stop(1, 1);
            Leisai_Axis.Leisai_Stop(2, 1);
        }
        /// <summary>
        /// �����Ƭ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_orientation_up_Click(object sender, EventArgs e)
        {

            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           _cts = new CancellationTokenSource();
                           var slot = cbx_Slot.Text;
                           int deviceIndex = 0; // U1Ϊ0��U2Ϊ1����������
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("����ѡ���豸��ţ�");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // ����ӽ���ѡ��
                           var coord = new Positions();
                           var biename = "";
                           if (string.IsNullOrEmpty(slot))
                           {
                               coord = ConfigService.FindPosition(deviceCode);
                               biename = ConfigService.FindbieName(deviceCode);
                           }
                           else
                           {
                               coord = ConfigService.FindPosition(deviceCode, slot);
                               biename = ConfigService.FindbieName(deviceCode, slot);
                           }
                           deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
                           if (deviceIndex < 0)
                           {
                               MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                               return false;
                           }
                           var plcConn = _coordinator?.GetPlcConnection(1); // 1Ϊ��PLCվ��

                           //if (plcConn == null)
                           //{
                           //    MessageBox.Show("PLC����δ��ʼ����", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                           //    return;
                           //}
                           //if (!plcConn.IsConnected)
                           //    await plcConn.ConnectAsync();
                           //_executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor, _dbService);

                           try
                           {
                               await PackageMove.OneKeyGetPut(coord, biename, slot, false);
                               //await _executor.GetRunAsync(_cts.Token);
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
                           return true; // ��Ҫ��������ֵ
                       }
                   );
            
        }
        /// <summary>
        /// ����ȡƬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_orientation_dow_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           _cts = new CancellationTokenSource();
                           int deviceIndex = 0; // U1Ϊ0��U2Ϊ1����������
                           var slot = cbx_Slot.Text;
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("����ѡ���豸��ţ�");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // ����ӽ���ѡ��
                           deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
                           if (deviceIndex < 0)
                           {
                               MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                               return false;
                           }
                           var plcConn = _coordinator?.GetPlcConnection(1); // 1Ϊ��PLCվ��
                           var coord = new Positions();
                           if (string.IsNullOrEmpty(slot))
                           {
                               coord = ConfigService.FindPosition(deviceCode);
                           }
                           else
                           {
                               coord = ConfigService.FindPosition(deviceCode, slot);
                           }
                           //if (plcConn == null)
                           //{
                           //    MessageBox.Show("PLC����δ��ʼ����", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                           //    return;
                           //}
                           //if (!plcConn.IsConnected)
                           //    await plcConn.ConnectAsync();
                           //_executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor, _dbService);

                           try
                           {

                               string biename = "";
                               if (string.IsNullOrEmpty(slot))
                               {
                                   coord = ConfigService.FindPosition(deviceCode);
                                   biename = ConfigService.FindbieName(deviceCode);
                               }
                               else
                               {
                                   coord = ConfigService.FindPosition(deviceCode, slot);
                                   biename = ConfigService.FindbieName(deviceCode, slot);
                               }
                               var sta = StationName.ChangeStaName(biename);
                               if (slot == null || slot == "")
                               {
                                   slot = "1";
                               }
                               await PackageMove.OneKeyGetPut(coord, biename, slot, true);
                               //await _executor.GetRunAsync(_cts.Token);
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
                           return true; // ��Ҫ��������ֵ
                       }
                   );
            
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
        /// <summary>
        /// ����ʦģʽ/��ͣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_GCStop_Click(object sender, EventArgs e)
        {
            Leisai_Axis.Leisai_Stop(0, 1);
            Leisai_Axis.Leisai_Stop(1, 1);
            Leisai_Axis.Leisai_Stop(2, 1);
        }
        /// <summary>
        /// ����ʦģʽ/��λ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_GCReset_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           await Reset();
                           return true; // ��Ҫ��������ֵ
                       },
                       "ϵͳ��λ��\n���Ժ�..."
                   );
        }
        /// <summary>
        /// ��λ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_Reset_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           await Reset();
                           return true; // ��Ҫ��������ֵ
                       },
                       "ϵͳ��λ��\n���Ժ�..."
                   );
        }
        /// <summary>
        /// ��λ��ԭ��
        /// </summary>
        /// <returns></returns>
        private async Task Reset()
        {
            CancellationToken cts = new CancellationToken();
            await HWRobot.Robot_InitAsync(cts);
            // Y���Z�Ტ�л���
            bool homeY_State = await Leisai_Home((ushort)AxisName.Y); // Y��
            await Leisai_Axis_Y_SafetyPoint_Pmov();
            bool homeZ_State = await Leisai_Home((ushort)AxisName.Z); // Z��

            Task isAlignerInit = HWAligner.Aligner_InitAsync(cts);
            Task isLoadportInit_1 = HWLoadPort_1.LoadPort1_InitAsync(cts);
            Task isLoadportInit_2 = HWLoadPort_2.LoadPort2_InitAsync(cts);
            Task isOCR_Aligner = OCR_Aligner.OCR_Aligner_InitAsync();
            Task isOCR_AngleT = OCR_AngleT.OCR_AngleT_InitAsync();
            await Task.WhenAll(isAlignerInit, isLoadportInit_1, isLoadportInit_2, isOCR_Aligner, isOCR_AngleT);

            if (homeZ_State && homeY_State)
            {
                // X����㣨������Y���Z����ɣ�
                bool homeX_State = await Leisai_Home((ushort)AxisName.X); // X��
                await Leisai_Axis.Leisai_Pmov((ushort)AxisName.X, 100, 1);

            }
            else
            {
                MessageBox.Show("Y���Z�����ʧ�ܣ�������״̬��");
            }
        }
        /// <summary>
        /// �����ƶ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_moveget_Click(object sender, EventArgs e)
        {

            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           int deviceIndex = 0; // U1Ϊ0��U2Ϊ1����������
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("����ѡ���豸��ţ�");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // ����ӽ���ѡ��

                           factoryLayoutControl1.SmoothMoveRobotToDevice(deviceCode);
                           factoryLayoutControl2.SmoothMoveRobotToDevice(deviceCode);

                           var coord = new Positions();
                           var slot = cbx_Slot.Text;
                           var biename = "";
                           if (string.IsNullOrEmpty(slot))
                           {
                               coord = ConfigService.FindPosition(deviceCode);
                               biename = ConfigService.FindbieName(deviceCode);
                           }
                           else
                           {
                               coord = ConfigService.FindPosition(deviceCode, slot);
                               biename = ConfigService.FindbieName(deviceCode, slot);
                           }
                           deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
                           if (deviceIndex < 0)
                           {
                               MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                               return false;
                           }
                           var robotManager = HWRobot._robot.IsConnected;
                           if (robotManager)
                           {
                               //CancellationToken cts = new CancellationToken();
                               //System.Enum.TryParse<Station>(deviceCode, out Station station);
                               //await Leisai_Axis.AxisMovStation(deviceCode, coord);
                               //await Task.Delay(500);
                               await PackageMove.MoveGetReadyPos(coord, biename, slot, true);
                           }
                           else
                           {
                               LogManager.Log("ROBOT: ������δ���ӣ��޷�ִ��׼������", LogLevel.Warn, "Robot.Main");
                               return false;
                           }
                           return true; // ��Ҫ��������ֵ
                       }
                   );
            

        }

        private async void but_moveput_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           int deviceIndex = 0; // U1Ϊ0��U2Ϊ1����������
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("����ѡ���豸��ţ�");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // ����ӽ���ѡ��

                           factoryLayoutControl1.SmoothMoveRobotToDevice(deviceCode);
                           factoryLayoutControl2.SmoothMoveRobotToDevice(deviceCode);

                           var coord = new Positions();
                           var slot = cbx_Slot.Text;
                           var biename = "";
                           if (string.IsNullOrEmpty(slot))
                           {
                               coord = ConfigService.FindPosition(deviceCode);
                               biename = ConfigService.FindbieName(deviceCode);
                           }
                           else
                           {
                               coord = ConfigService.FindPosition(deviceCode, slot);
                               biename = ConfigService.FindbieName(deviceCode, slot);
                           }
                           deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
                           if (deviceIndex < 0)
                           {
                               MessageBox.Show($"PLC ������δ�ҵ��豸���: {deviceCode}");
                               return false;
                           }
                           var robotManager = HWRobot._robot.IsConnected;
                           if (robotManager)
                           {
                               //CancellationToken cts = new CancellationToken();
                               //System.Enum.TryParse<Station>(deviceCode, out Station station);
                               //await Leisai_Axis.AxisMovStation(deviceCode, coord);
                               //await Task.Delay(500);
                               await PackageMove.MoveGetReadyPos(coord, biename, slot, false);
                           }
                           else
                           {
                               LogManager.Log("ROBOT: ������δ���ӣ��޷�ִ��׼������", LogLevel.Warn, "Robot.Main");
                               return false;
                           }
                           return true; // ��Ҫ��������ֵ
                       }
                   );

            

        }
        /// <summary>
        /// ����ʦģʽ/�ϵ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_GCPowered_Click(object sender, EventArgs e)
        {
            bool initSuccess = await _initializer.InitializeAsync();
            if (!initSuccess)
            {
                uiLight1.State = UILightState.Off;
                uiLight2.State = UILightState.Off;
                LogManager.Log("���ӳ�ʼ��ʧ��", LogLevel.Error);
            }
        }
        /// <summary>
        /// ж��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void uiButton2_Click(object sender, EventArgs e)
        {
            if (LeisaiIO.Robot_Ready && !LeisaiIO.Robot_Fault)
            {

                var coord = new Positions();
                var biename = "";
                if (uiRadioButton1.Checked)
                {
                    coord = ConfigService.FindPosition("LP1", "1");
                    biename = ConfigService.FindbieName("LP1", "1");
                    await HWLoadPort_1.LoadPort1_LoadAsync();
                    if (wafer_type.Text == "Square")
                    {
                        var xRdy = Leisai_Axis.Leisai_CheckDone(0);
                        var yRdy = Leisai_Axis.Leisai_CheckDone(1);
                        var xRdz = Leisai_Axis.Leisai_CheckDone(2);
                        if (xRdy == 0 && yRdy == 0 && xRdz == 0)
                        {
                            var sta = StationName.ChangeStaName(biename);
                            await PackageMove.MapAsync(sta, coord);
                        }
                    }

                }
                else if (uiRadioButton2.Checked)
                {

                    coord = ConfigService.FindPosition("LP2", "1");
                    biename = ConfigService.FindbieName("LP2", "1");
                    await HWLoadPort_2.LoadPort2_LoadAsync();
                    if (wafer_type.Text == "Square")
                    {
                        var xRdy = Leisai_Axis.Leisai_CheckDone(0);
                        var yRdy = Leisai_Axis.Leisai_CheckDone(1);
                        var xRdz = Leisai_Axis.Leisai_CheckDone(2);
                        if (xRdy == 0 && yRdy == 0 && xRdz == 0)
                        {
                            var sta = StationName.ChangeStaName(biename);
                            await PackageMove.MapAsync(sta, coord);
                        }
                    }
                }

            }
            else
            {
                LogManager.Log("Robot����Run״̬", LogLevel.Warn);
            }

        }
        /// <summary>
        /// װ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void uiButton3_Click(object sender, EventArgs e)
        {
            if (uiRadioButton1.Checked)
            {
                await HWLoadPort_1.LoadPort1_UnloadAsync();
            }
            else if (uiRadioButton2.Checked)
            {
                await HWLoadPort_2.LoadPort2_UnloadAsync();
            }
        }
        /// <summary>
        /// ɨƬװ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_MapLoad_Click(object sender, EventArgs e)
        {

            if (uiRadioButton1.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y�����ƶ�����ȫλ");
                await HWLoadPort_1.LoadPort1_MapLoadAsync();

                await HWLoadPort_1.LP1ReadMappingAsync();
            }
            else if (uiRadioButton2.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y�����ƶ�����ȫλ");
                await HWLoadPort_2.LoadPort2_MapLoadAsync();

                await HWLoadPort_2.LP2ReadMappingAsync();
            }
        }
        /// <summary>
        /// ɨƬж��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_MapUnLoad_Click(object sender, EventArgs e)
        {
            if (uiRadioButton1.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y�����ƶ�����ȫλ");
                await HWLoadPort_1.LoadPort1_UnMaploadAsync();

                await HWLoadPort_1.LP1ReadMappingAsync();
            }
            else if (uiRadioButton2.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y�����ƶ�����ȫλ");
                await HWLoadPort_2.LoadPort2_UnMaploadAsync();

                await HWLoadPort_2.LP2ReadMappingAsync();
            }
        }
        /// <summary>
        /// �ƶ�������λ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_MTM_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           if (cbx_Aligner.Text == "Aligner")
                           {
                               await HWAligner.Aligner_MoveToCenter();
                           }
                           else if (cbx_Aligner.Text == "AngleT")
                           {
                               await AngleT.MTMAsync();
                           }
                           return true; // ��Ҫ��������ֵ
                       },
                       "Ѱ������\n���Ժ�..."
                   );
            
        }
        /// <summary>
        /// Ѱ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_BAL_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           if (cbx_Aligner.Text == "Aligner")
                           {
                               await HWAligner.Aligner_BAL();

                           }
                           else if (cbx_Aligner.Text == "AngleT")
                           {
                               await AngleT.AngleTOneKeyBALAsync();
                           }
                           return true; // ��Ҫ��������ֵ
                       },
                       "Ѱ�ұ�Ե\n���Ժ�..."
                   );
            
        }
        /// <summary>
        /// ��ȡCode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_OCRTirg_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           if (cbx_OCR.Text == "Aligner")
                           {
                               var ret = await OCR_Aligner.OCR_Aligner_Trig();
                               txt_WaferID.Text = ret;
                           }
                           else if (cbx_OCR.Text == "AngleT")
                           {
                               var ret = await OCR_AngleT.OCR_AngleT_Trig();
                               txt_WaferID.Text = ret;
                           }
                           return true; // ��Ҫ��������ֵ
                       },
                       "��ȡCode\n���Ժ�..."
                   );
            
        }
        /// <summary>
        /// Waferѡ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void panel8_Click(object sender, EventArgs e)
        {
            await WaferSelect();
        }

        private async Task WaferSelect()
        {
            using (WaferSelectionForm waferForm = new WaferSelectionForm(_currentWaferType, _currentWaferSize))
            {
                if (waferForm.ShowDialog() == DialogResult.OK)
                {
                    _currentWaferSize = waferForm.SelectedSize;
                    _currentWaferType = waferForm.SelectedWaferType;
                    // ��ʾѡ����
                    wafer_type.Text = waferForm.SelectedWaferType;
                    wafer_size.Text = waferForm.SelectedSize;

                    // �����������Զ���ʼ���߼�����
                    //StartAutoRun(waferForm.SelectedWaferType, waferForm.SelectedSize);

                    await WaferTypeGetStaName();
                }
                else
                {
                    MessageBox.Show("��ȡ���Զ���ʼ����");
                }
            }
        }

        public async Task WaferTypeGetStaName()
        {
            string groupSta = "";
            bool typ = wafer_type.Text == "Wafer";
            bool siz = wafer_size.Text == "8Ӣ��";
            if (typ && siz)
            {
                groupSta = StationName.GetWaferType(Group.Wafer8);
            }
            else if (typ && !siz)
            {
                groupSta = StationName.GetWaferType(Group.Wafer12);
            }
            else if (!typ && siz)
            {
                groupSta = StationName.GetWaferType(Group.Square8);
            }
            else if (!typ && !siz)
            {
                groupSta = StationName.GetWaferType(Group.Square12);
            }
            await HWRobot.SGRPAsync(groupSta);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ����IO��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (LeiSaiFrm leiSaiFrm = new LeiSaiFrm())
            {
                leiSaiFrm.ShowDialog();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void �˶����ƿ�ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void axisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AxisFrm axisFrm = new AxisFrm())
            {
                axisFrm.ShowDialog();
            }
        }
        #region �����������
        private async void ErrorClear_lp1_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            await HWLoadPort_1.LoadPort1_ClearingErrorAsync();
                            await HWLoadPort_1.LoadPort1_HomeAsync();
                            return true; // ��Ҫ��������ֵ
                        }
                    );
            
        }

        private async void ErrorClear_lp2_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            await HWLoadPort_2.LoadPort2_ClearingErrorAsync();
                            await HWLoadPort_2.LoadPort2_HomeAsync();
                            return true; // ��Ҫ��������ֵ
                        }
                    );
        }

        private async void ErrorClear_robot_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            await HWRobot.Robot_RemsAsync();
                            await HWRobot.Robot_SvonAsync();
                            return true; // ��Ҫ��������ֵ
                        }
                    );
        }

        private async void ErrorClear_Aligner_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            await HWAligner._aligner.ClearAlarmAsync();
                            await HWAligner.Aligner_Setting();
                            // await HWAligner.Aligner_InitAsync();
                            return true; // ��Ҫ��������ֵ
                        }
                    );
            
        }

        private async void ErrorClear_AngleT_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            await AngleT.AngleTInitAsync();
                            await AngleT.HomeAsync();
                            return true; // ��Ҫ��������ֵ
                        }
                    );
        }

        private void ErrorClear_Axis_Click(object sender, EventArgs e)
        {
            Leisai_Axis.Leisai_nmcClearErrcode();

        }

        #endregion


        private void uiButton2_Click_1(object sender, EventArgs e)
        {

        }
    }
}
