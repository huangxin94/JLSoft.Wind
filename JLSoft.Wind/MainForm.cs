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
        // 保留设计器生成的控件
        private float rotationAngle = 0; // 记录当前旋转角度

        public static string AxisConfigFilePath = Application.StartupPath + "\\Axis_Config.ini";// 运动控制卡配置文件路径
        public static MainForm _instance;



        private readonly DuckDbService _dbService;// 数据库服务实例

        // 在 MainForm 类中添加以下成员变量
        private bool _isUserLoggedIn = false;/// 是否已登录 
        CommunicationCoordinator _coordinator; //通信协调器多个PLC连接
        private DeviceMonitor _deviceMonitor; // 设备监控管理器
        private PlcConnection _plcConn;

        private static string _currentWaferSize = "8英寸"; // 默认8英寸
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

            // 4. 日志系统初始化
            LogManager.Initialize(uiRichTextBox1);
            try
            {
                // 初始化代码
                _dbService = new DuckDbService(Program.DuckDBConnectionString);
            }
            catch (TypeInitializationException ex)
            {
                // 打印内部异常详情
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

            当前登录用户ToolStripMenuItem.Text = "当前登录用户：未登录"; // 设置菜单项文本

            // 2. 初始化设备下拉框
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
                    loadPort21.UpdateMapping(mappingString); // 假设 LoadPort2 控件有 SetColor 方法
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
                    // 用户未登录，显示登录
                    LoginFrm loginForm = new LoginFrm();
                    loginForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
                    DialogResult result = loginForm.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        // 登录成功，获取用户角色
                        _isUserLoggedIn = true;
                        当前登录用户ToolStripMenuItem.Text = $"当前登录用户：{SessionManager.CurrentUser.Username} (职阶: {SessionManager.CurrentUser.Grade})"; // 更新菜单项文本
                        if (SessionManager.CurrentUser.Grade != "工程师" && SessionManager.CurrentUser.Grade != "超级管理员")
                        {
                            // 用户角色权限不足，提示信息并取消选择
                            MessageBox.Show("无权限访问该页面", "权限不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        // 登录取消，取消选择
                        e.Cancel = true;
                    }
                }
                else
                {
                    if (SessionManager.CurrentUser.Grade != "工程师" && SessionManager.CurrentUser.Grade != "超级管理员")
                    {
                        // 用户角色权限不足，提示信息并取消选择
                        MessageBox.Show("无权限访问该页面", "权限不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }
                */

                Application.DoEvents();
                factoryLayoutControl1.ForceRescale();
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 判断是否是有效的行点击（排除表头点击等情况）
            if (e.RowIndex >= 0)
            {

                string column0Data = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();
                // 获取第3、4、5、6列的数据

                // 实例化子页面（假设子页面的构造函数能接收这些数据）
                ActualProcessItemFrm sonForm = new ActualProcessItemFrm(_dbService, column0Data);
                sonForm.Text = "产品生产流程配置";  // 设置窗体标题
                sonForm.StartPosition = FormStartPosition.CenterScreen;
                sonForm.ShowDialog();
            }
        }
        private DataTable dataTable; // 声明为类成员以便访问
        private void InitializeDataGridView()
        {
            try
            {

                // 构建SQL和参数
                string sql = @"select
	                            a.mainid as 主键,
	                            a.Status as 状态,
	                            a.CurrentSite as 当前站点,
	                            a.Progress as 完成进度,
	                            a.type as 类型,
	                            a.ProductName as 产品名称,
	                            a.ProductCode as 产品编码,
	                            a.StartTime as 开始时间,
	                            a.EndTime as 完成时间,
	                            b.Qtime as 设定Qtime值,
	                            b.StartQtime as 开始计算Qtime时间,
	                            b.ActualQtime as 实际Qtime
                            from
	                            ProductiveProcessMain a
                            left join actualprocessitem b
                                on
	                            a.actualprocessitemid = b.actualprocessitemid
                            where
	                            a.Status = ?";
                var parameters = new List<DuckDB.NET.Data.DuckDBParameter>();
                parameters.Add(new DuckDB.NET.Data.DuckDBParameter { Value = "生产" });

                // 3. 查询数据库
                dataTable = _dbService.ExecuteQuery(sql, parameters.ToArray());

            }
            catch (Exception ex)
            {
                LogManager.Log($"生产主数据查询失败: {ex.Message}");
            }

            dataGridView1.DataSource = dataTable; // 直接绑定数据源，无需手动绘制


            dataGridView2.DataSource = dataTable; // 直接绑定数据源，无需手动绘制
            if (dataTable != null)
            {
                dataGridView1.Columns["主键"].Visible = false;
                dataGridView2.Columns["主键"].Visible = false;
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

        // 旋转图片（顺时针90度）
        private void RotateImage()
        {

        }
        private const int MoveStep = 10; // 每次移动的步长
        private void button1_Click(object sender, EventArgs e)
        {
            RotateImage();
        }
        /// <summary>
        /// 左移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            MovePictureBox(-MoveStep);
        }
        /// <summary>
        /// 右移
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
        private void 设备定位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            RobotSettingFrm settingFrm = new RobotSettingFrm();
            settingFrm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            settingFrm.ShowDialog(this);

        }

        private void 当前流程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            ProcessFlowFrm sonForm = new ProcessFlowFrm(_dbService);
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.ShowDialog(this);
        }

        private void alignerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "aligner";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个aligner查看/操作页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("aligner已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
        }

        private void alignerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "角度台";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个角度台查看/操作页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("角度台已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
        }

        private void lDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "LD";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个LD查看/操作页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("LD已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void p1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "P1";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个P1查看/操作页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("P1已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CustomMessageBox.Show("P1已完成", "成功", MessageType.Warning);
            }
        }

        private void x1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "X1";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个X1查看/操作页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("X1已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void 寻边器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "寻边器";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个寻边器查看/操作页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("寻边器已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void 报警LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "报警Log";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个报警Log查看页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("报警Log已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void 日产能ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 实例化已有的子页面窗体
            SonForm sonForm = new SonForm();

            // 设置窗体属性（可选）
            sonForm.Text = "日产能";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            sonForm.label1.Text = "这是一个日产能查看页面"; // 设置标签文本
            // 显示窗体（两种方式可选）

            // 方式1：以模态对话框形式显示（用户必须先关闭子窗体才能操作主窗体）
            DialogResult result = sonForm.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                // 处理"确定"操作（如果子窗体有确认按钮）
                MessageBox.Show("日产能已完成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                // 自定义处理逻辑
                if (e.TargetControl is DeviceBlock block)
                {
                    string deviceCode = block.DeviceCode;
                    DeviceStateFrm devicestateFrm = new DeviceStateFrm(deviceCode);
                    devicestateFrm.Text = deviceCode;  // 设置窗体标题
                    devicestateFrm.textBox1.Text = deviceCode; // 设置设备名称
                    devicestateFrm.StartPosition = FormStartPosition.CenterParent;
                    devicestateFrm.ShowDialog(this);
                }
            }

            if (!string.IsNullOrEmpty(e.ErrorMessage))
            {
                MessageBox.Show($"操作错误: {e.ErrorMessage}");
            }
        }

        private void OnColorChanged(object sender, FactoryLayoutControl.ControlOperationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ErrorMessage))
            {
                MessageBox.Show($"颜色错误: {e.ErrorMessage}");
            }
        }
        /// <summary>
        /// 进入工程师管理页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 用户管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManagementUsersFrm management = new ManagementUsersFrm(_dbService);
            management.StartPosition = FormStartPosition.CenterParent;  // 居中显示
            DialogResult result = management.ShowDialog(this);
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 退出登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SessionManager.CurrentUser == null)
            {
                MessageBox.Show("当前没有登录用户。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult result = MessageBox.Show("是否确认退出登录状态？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                当前登录用户ToolStripMenuItem.Text = "当前登录用户：未登录"; // 更新菜单项文本
                _isUserLoggedIn = false;
                SessionManager.Logout();// 清除当前用户会话
            }
        }

        private void 当前登录用户ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SessionManager.CurrentUser == null)
            {

                LoginFrm loginForm = new LoginFrm();
                loginForm.StartPosition = FormStartPosition.CenterParent;  // 居中显示
                DialogResult result = loginForm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // 登录成功，获取用户角色
                    _isUserLoggedIn = true;
                    当前登录用户ToolStripMenuItem.Text = $"当前登录用户：{SessionManager.CurrentUser.Username} (职阶: {SessionManager.CurrentUser.Grade})"; // 更新菜单项文本
                }
            }

        }
        public MitsubishiPLC plc;
        /// <summary>
        /// 上电
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
                LogManager.Log("连接初始化失败", LogLevel.Error);
            }
            //MessageBox.Show(uiRichTextBox1.Text.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private SequenceTaskExecutor _executor;
        private CancellationTokenSource _cts;
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_readin_Click(object sender, EventArgs e)
        {
            /*
            _cts = new CancellationTokenSource();
            int deviceIndex = (int)CbxFacility.SelectedValue; // U1为0，U2为1，依次类推
            string deviceCode = (string)CbxFacility.SelectedValue; // 例如从界面选择
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
                return;
            }
            var plcConn = _coordinator?.GetPlcConnection(1); // 1为主PLC站号

            if (plcConn == null)
            {
                MessageBox.Show("PLC连接未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, "", new Positions(), _deviceMonitor, _dbService);

            try
            {
                await _executor.PutRunAsync(_cts.Token);
                LogManager.Log($"时序任务{deviceIndex}完成！");
            }
            catch (OperationCanceledException)
            {
                LogManager.Log($"时序任务{deviceIndex}被取消。", LogLevel.Warn);
            }
            catch (Exception ex)
            {
                LogManager.Log($"时序任务{deviceIndex}异常: {ex.Message}", LogLevel.Error);
            }
            */

            await WaferSelect();
        }
        /// <summary>
        /// 读取
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
            // 订阅初始化完成事件
            _initializer.InitializationCompleted += OnSystemInitialized;

            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            // 开始初始化
                            bool initSuccess = await _initializer.InitializeAsync();
                            if (!initSuccess)
                            {
                                uiLight1.State = UILightState.Off;
                                uiLight2.State = UILightState.Off;
                                LogManager.Log("连接初始化失败", LogLevel.Error);
                            }
                            if (LeisaiIO.leisaiIO_Init())
                            {
                                LedControl.LedFirst();
                                StartIOInRefresh(50);
                                StartIOOutRefresh(100);
                                StartAxisPosition(10);
                            }
                            return true; // 需要返回任意值
                        },
                        "系统初始化中\n请稍候..."
                    );
        }

        private void StartIOInRefresh(int v)
        {
            // 确保定时器存在
            if (_inputRefreshTimer == null)
            {
                _inputRefreshTimer = new Timer();
                _inputRefreshTimer.Tick += InputRefreshTimer_Tick;
            }

            // 配置定时器参数
            _inputRefreshTimer.Interval = v;

            // 启动定时器
            _inputRefreshTimer.Start();
        }
        private void InputRefreshTimer_Tick(object sender, EventArgs e)
        {
            LeisaiIO.ReadInputState();
        }

        private void StartIOOutRefresh(int v)
        {
            // 确保定时器存在
            if (_outputRefreshTimer == null)
            {
                _outputRefreshTimer = new Timer();
                _outputRefreshTimer.Tick += OutputRefreshTimer_Tick;
            }

            // 配置定时器参数
            _outputRefreshTimer.Interval = v;

            // 启动定时器
            _outputRefreshTimer.Start();
        }

        private void OutputRefreshTimer_Tick(object sender, EventArgs e)
        {
            LeisaiIO.ReadOutputState();
        }

        /// <summary>
        /// 读取轴位置timer
        /// </summary>
        /// <param name="v"></param>
        private void StartAxisPosition(int v)
        {
            if (_positionRefreshTimer == null)
            {
                _positionRefreshTimer = new Timer();
                _positionRefreshTimer.Tick += PositionRefreshTimer_Tick;
            }

            // 配置定时器参数
            _positionRefreshTimer.Interval = v;

            // 启动定时器
            _positionRefreshTimer.Start();
        }

        /// <summary>
        /// 读取位置并显示UI
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
            // 1. 更新UI状态
            uiLight1.State = _initializer.IsRobotConnected
                ? UILightState.On : UILightState.Off;

            uiLight2.State = _initializer.IsPlcConnected
                ? UILightState.On : UILightState.Off;



            // 3. 绑定设备监控事件
            _deviceMonitor = _initializer.GetDeviceMonitor();
            _plcConn = _initializer.GetPlcConnection();
            if (_deviceMonitor != null)
            {
                _deviceMonitor.DeviceStatesUpdated += OnDeviceStatesUpdated;

                // 立即触发一次状态更新
                var currentStates = _deviceMonitor.CurrentDeviceStates;
                if (currentStates != null)
                {
                    OnDeviceStatesUpdated(new Dictionary<string, DeviceMonitor.DeviceState>(currentStates));
                }
                else
                {
                    // 如果当前状态为空，手动设置所有设备为离线
                    SetAllDevicesToOfflineUI();
                }
            }
            else
            {
                // 设备监控为空时，设置所有设备为离线
                SetAllDevicesToOfflineUI();
            }

            LogManager.Log("系统初始化完成", LogLevel.Info);
            uiButton1.BringToFront();
        }

        // 新增方法：设置所有设备UI为离线状态
        private void SetAllDevicesToOfflineUI()
        {
            try
            {
                var allDeviceCodes = new[] { "V1", "U1", "S3", "M1", "S4", "C1", "R1", "G1",
                                   "A1", "A2", "A3", "A4", "寻边机", "角度台", "LP1", "LP2" };

                foreach (var deviceCode in allDeviceCodes)
                {
                    factoryLayoutControl1.SetDeviceBlockBorderColor(deviceCode, DeviceStatus.Offline);
                    factoryLayoutControl2.SetDeviceBlockBorderColor(deviceCode, DeviceStatus.Offline);
                }

                // 更新连接指示灯
                uiLight2.State = UILightState.Off;
            }
            catch (Exception ex)
            {
                LogManager.Log($"设置设备离线UI失败: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnDeviceStatesUpdated(Dictionary<string, DeviceMonitor.DeviceState> deviceStates)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnDeviceStatesUpdated(deviceStates));
                return;
            }

            // 获取DeviceMonitor的运行模式信息
            bool isOfflineMode = _deviceMonitor?.IsOfflineMode ?? true;
            bool isPlcConnected = _deviceMonitor?.IsPlcConnected ?? false;

            // 根据运行模式更新设备UI
            if (!isPlcConnected || isOfflineMode)
            {
                SetAllDevicesToOfflineUI();
                return;
            }

            // 正常更新设备状态
            if (deviceStates != null)
            {
                foreach (var (deviceCode, state) in deviceStates)
                {
                    UpdateDeviceUI(deviceCode, state);
                }
            }

            // 更新连接指示器
            UpdateConnectionIndicators(isPlcConnected, isOfflineMode);
        }


        // 新增方法：更新连接指示灯
        private void UpdateConnectionIndicators(bool isPlcConnected, bool isOfflineMode)
        {
            if (!isPlcConnected)
            {
                uiLight2.State = UILightState.Off; // PLC连接灯熄灭
            }
            else if (isOfflineMode)
            {
                uiLight2.State = UILightState.Blink; // PLC连接但处于离线模式，闪烁
            }
            else
            {
                uiLight2.State = UILightState.On; // PLC正常连接
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

            // 显示报警状态
            factoryLayoutControl2.SetDeviceBlockBorderColor(deviceCode, deviceStatus);

            uiLight1.State = (RobotManager.Instance.IsConnected ? UILightState.On : UILightState.Off);
            uiLight2.State = (_plcConn != null && _plcConn.IsConnected) ? UILightState.On : UILightState.Off;

        }
        /// <summary>
        /// 清除日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label2_Click(object sender, EventArgs e)
        {
            this.uiRichTextBox1.Text = string.Empty; // 清空日志
        }
        /// <summary>
        /// 停止
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
        /// 定点放片
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
                           int deviceIndex = 0; // U1为0，U2为1，依次类推
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("请先选择设备编号！");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // 例如从界面选择
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
                               MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
                               return false;
                           }
                           var plcConn = _coordinator?.GetPlcConnection(1); // 1为主PLC站号

                           //if (plcConn == null)
                           //{
                           //    MessageBox.Show("PLC连接未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                           //    return;
                           //}
                           //if (!plcConn.IsConnected)
                           //    await plcConn.ConnectAsync();
                           //_executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor, _dbService);

                           try
                           {
                               await PackageMove.OneKeyGetPut(coord, biename, slot, false);
                               //await _executor.GetRunAsync(_cts.Token);
                               LogManager.Log($"时序任务{deviceIndex}完成！");
                           }
                           catch (OperationCanceledException)
                           {
                               LogManager.Log($"时序任务{deviceIndex}被取消。", LogLevel.Warn);
                           }
                           catch (Exception ex)
                           {
                               LogManager.Log($"时序任务{deviceIndex}异常: {ex.Message}", LogLevel.Error);
                           }
                           return true; // 需要返回任意值
                       }
                   );
            
        }
        /// <summary>
        /// 定点取片
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
                           int deviceIndex = 0; // U1为0，U2为1，依次类推
                           var slot = cbx_Slot.Text;
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("请先选择设备编号！");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // 例如从界面选择
                           deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
                           if (deviceIndex < 0)
                           {
                               MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
                               return false;
                           }
                           var plcConn = _coordinator?.GetPlcConnection(1); // 1为主PLC站号
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
                           //    MessageBox.Show("PLC连接未初始化！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                               LogManager.Log($"时序任务{deviceIndex}完成！");
                           }
                           catch (OperationCanceledException)
                           {
                               LogManager.Log($"时序任务{deviceIndex}被取消。", LogLevel.Warn);
                           }
                           catch (Exception ex)
                           {
                               LogManager.Log($"时序任务{deviceIndex}异常: {ex.Message}", LogLevel.Error);
                           }
                           return true; // 需要返回任意值
                       }
                   );
            
        }

        private void CbxFacility_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (CbxFacility.SelectedItem is DeviceIndices selectedDevice)
            {
                if (selectedDevice.SubStations != null && selectedDevice.SubStations.Any())
                {
                    // 绑定子站
                    cbx_Slot.DataSource = selectedDevice.SubStations;
                    cbx_Slot.DisplayMember = "Name";  // 显示子站名称
                    cbx_Slot.ValueMember = "Name";    // 值取子站名称
                    cbx_Slot.Enabled = true;
                }
                else
                {
                    // 没有子站：清空ComboBox并禁用
                    cbx_Slot.DataSource = null; // 清除数据源
                    cbx_Slot.Items.Clear();     // 清除所有项
                    cbx_Slot.Text = string.Empty; // 清空显示文本
                    cbx_Slot.Enabled = false;
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 设备信号查看ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EquipmentStatusFrm sonForm = new EquipmentStatusFrm(_deviceMonitor);
            sonForm.Text = "设备信号查看";  // 设置窗体标题
            sonForm.StartPosition = FormStartPosition.CenterScreen;
            sonForm.ShowDialog(this);
        }

        private void 测试ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 工程师模式/急停
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
        /// 工程师模式/复位
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
                           return true; // 需要返回任意值
                       },
                       "系统复位中\n请稍候..."
                   );
        }
        /// <summary>
        /// 复位
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
                           return true; // 需要返回任意值
                       },
                       "系统复位中\n请稍候..."
                   );
        }
        /// <summary>
        /// 复位回原点
        /// </summary>
        /// <returns></returns>
        private async Task Reset()
        {
            CancellationToken cts = new CancellationToken();
            await HWRobot.Robot_InitAsync(cts);
            // Y轴和Z轴并行回零
            bool homeY_State = await Leisai_Home((ushort)AxisName.Y); // Y轴
            await Leisai_Axis_Y_SafetyPoint_Pmov();
            bool homeZ_State = await Leisai_Home((ushort)AxisName.Z); // Z轴

            Task isAlignerInit = HWAligner.Aligner_InitAsync(cts);
            Task isLoadportInit_1 = HWLoadPort_1.LoadPort1_InitAsync(cts);
            Task isLoadportInit_2 = HWLoadPort_2.LoadPort2_InitAsync(cts);
            Task isOCR_Aligner = OCR_Aligner.OCR_Aligner_InitAsync();
            Task isOCR_AngleT = OCR_AngleT.OCR_AngleT_InitAsync();
            await Task.WhenAll(isAlignerInit, isLoadportInit_1, isLoadportInit_2, isOCR_Aligner, isOCR_AngleT);

            if (homeZ_State && homeY_State)
            {
                // X轴回零（依赖于Y轴和Z轴完成）
                bool homeX_State = await Leisai_Home((ushort)AxisName.X); // X轴
                await Leisai_Axis.Leisai_Pmov((ushort)AxisName.X, 100, 1);

            }
            else
            {
                MessageBox.Show("Y轴或Z轴回零失败，请检查轴状态。");
            }
        }
        /// <summary>
        /// 单步移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_moveget_Click(object sender, EventArgs e)
        {

            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           int deviceIndex = 0; // U1为0，U2为1，依次类推
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("请先选择设备编号！");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // 例如从界面选择

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
                               MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
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
                               LogManager.Log("ROBOT: 机器人未连接，无法执行准备动作", LogLevel.Warn, "Robot.Main");
                               return false;
                           }
                           return true; // 需要返回任意值
                       }
                   );
            

        }

        private async void but_moveput_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                       this,
                       async (ct) =>
                       {
                           int deviceIndex = 0; // U1为0，U2为1，依次类推
                           if (CbxFacility.Text == "")
                           {
                               MessageBox.Show("请先选择设备编号！");
                               return false;
                           }
                           string deviceCode = CbxFacility.Text; // 例如从界面选择

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
                               MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
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
                               LogManager.Log("ROBOT: 机器人未连接，无法执行准备动作", LogLevel.Warn, "Robot.Main");
                               return false;
                           }
                           return true; // 需要返回任意值
                       }
                   );

            

        }
        /// <summary>
        /// 工程师模式/上电
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
                LogManager.Log("连接初始化失败", LogLevel.Error);
            }
        }
        /// <summary>
        /// 卸载
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
                LogManager.Log("Robot处于Run状态", LogLevel.Warn);
            }

        }
        /// <summary>
        /// 装载
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
        /// 扫片装载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_MapLoad_Click(object sender, EventArgs e)
        {

            if (uiRadioButton1.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y轴已移动至安全位");
                await HWLoadPort_1.LoadPort1_MapLoadAsync();

                await HWLoadPort_1.LP1ReadMappingAsync();
            }
            else if (uiRadioButton2.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y轴已移动至安全位");
                await HWLoadPort_2.LoadPort2_MapLoadAsync();

                await HWLoadPort_2.LP2ReadMappingAsync();
            }
        }
        /// <summary>
        /// 扫片卸载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_MapUnLoad_Click(object sender, EventArgs e)
        {
            if (uiRadioButton1.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y轴已移动至安全位");
                await HWLoadPort_1.LoadPort1_UnMaploadAsync();

                await HWLoadPort_1.LP1ReadMappingAsync();
            }
            else if (uiRadioButton2.Checked)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y轴已移动至安全位");
                await HWLoadPort_2.LoadPort2_UnMaploadAsync();

                await HWLoadPort_2.LP2ReadMappingAsync();
            }
        }
        /// <summary>
        /// 移动至中心位置
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
                           return true; // 需要返回任意值
                       },
                       "寻找中心\n请稍候..."
                   );
            
        }
        /// <summary>
        /// 寻边
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
                           return true; // 需要返回任意值
                       },
                       "寻找边缘\n请稍候..."
                   );
            
        }
        /// <summary>
        /// 读取Code
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
                           return true; // 需要返回任意值
                       },
                       "读取Code\n请稍候..."
                   );
            
        }
        /// <summary>
        /// Wafer选择
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
                    // 显示选择结果
                    wafer_type.Text = waferForm.SelectedWaferType;
                    wafer_size.Text = waferForm.SelectedSize;

                    // 这里可以添加自动开始的逻辑代码
                    //StartAutoRun(waferForm.SelectedWaferType, waferForm.SelectedSize);

                    await WaferTypeGetStaName();
                }
                else
                {
                    MessageBox.Show("已取消自动开始操作");
                }
            }
        }

        public async Task WaferTypeGetStaName()
        {
            string groupSta = "";
            bool typ = wafer_type.Text == "Wafer";
            bool siz = wafer_size.Text == "8英寸";
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
        private void 雷赛IO卡ToolStripMenuItem_Click(object sender, EventArgs e)
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
        private void 运动控制卡ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void axisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AxisFrm axisFrm = new AxisFrm())
            {
                axisFrm.ShowDialog();
            }
        }
        #region 清除报警操作
        private async void ErrorClear_lp1_Click(object sender, EventArgs e)
        {
            await AsyncOperations.RunWithLoading(
                        this,
                        async (ct) =>
                        {
                            await HWLoadPort_1.LoadPort1_ClearingErrorAsync();
                            await HWLoadPort_1.LoadPort1_HomeAsync();
                            return true; // 需要返回任意值
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
                            return true; // 需要返回任意值
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
                            return true; // 需要返回任意值
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
                            return true; // 需要返回任意值
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
                            return true; // 需要返回任意值
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
