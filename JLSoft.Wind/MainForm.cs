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
        // 保留设计器生成的控件
        private List<IndustrialModule> modules = new List<IndustrialModule>();// 工业模块列表
        private float rotationAngle = 0; // 记录当前旋转角度

        private readonly DuckDbService _dbService;// 数据库服务实例
        private UIRichTextBox logBox;// 日志框

        // 在 MainForm 类中添加以下成员变量
        private bool _isUserLoggedIn = false;/// 是否已登录 
        private TimechartService _timechartService;//
        CommunicationCoordinator _coordinator; //通信协调器多个PLC连接
        private DeviceMonitor _deviceMonitor; // 设备监控管理器
        private PlcConnection _plcConn;
        public static List<PositionInfo> AllPositions { get; private set; }

        public MainForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
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

        }

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == tabPage2)
            {
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

        private void uiDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
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

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
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
            }

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
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

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
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

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
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

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
            // sonForm.Show(this);
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

            // 方式2：以非模态形式显示（用户可同时操作主窗体和子窗体）
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
            but_uppower.Enabled = false;
            but_uppower.Text = "连接中...";
            try
            {
                plc = MitsubishiPLC.Instance;
                var result = await plc.ConnectAsync();

                if (result.IsSuccess)
                {
                    but_uppower.Text = "已连接";
                    but_uppower.FillColor = Color.Green;
                    LogManager.Log("PLC连接成功",LogLevel.Info, "PLC.Main");
                }
                else
                {
                    but_uppower.Text = "上电";
                    LogManager.Log($"PLC连接失败: {result.Message}",LogLevel.Info, "PLC.Main");
                }
            }
            catch (Exception ex)
            {
                but_uppower.Text = "上电";
                LogManager.Log($"连接异常: {ex.Message}");
            }
            finally
            {
                but_uppower.Enabled = true;
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

            _cts = new CancellationTokenSource();
            int deviceIndex = (int)CbxFacility.SelectedValue; // U1为0，U2为1，依次类推
            string deviceCode = (string)CbxFacility.SelectedValue; // 例如从界面选择
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
                return;
            }
            var plcConn = _coordinator.GetPlcConnection(1); // 1为主PLC站号

            if (plcConn == null)
            {
                MessageBox.Show("PLC连接未初始化！");
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, "", new Positions(), _deviceMonitor);

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
            /*
            try
            {
                but_readin.Enabled = false;
                plc = _coordinator.GetPlcConnection(1).Plc;
                if (plc == null) plc = MitsubishiPLC.Instance;

                if (!plc.IsConnected)
                {
                    LogManager.Log("PLC未连接，正在尝试连接...");
                    await plc.ConnectAsync();
                }

                var writeResult = await plc.WriteDataAsync("B100", 0);
                if (!writeResult.IsSuccess)
                    LogManager.Log($"写入失败: {writeResult.Message}");

                short[] values = { 0, 0, 0 };
                var batchResult = await plc.WriteDataAsync("B200", values);

                LogManager.Log(batchResult.IsSuccess ?
                    "批量写入成功" : $"批量写入失败: {batchResult.Message}");
            }
            catch (Exception ex)
            {
                LogManager.Log($"写入操作异常: {ex.Message}");
            }
            finally
            {
                but_readin.Enabled = true;
            }
            */
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_readout_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // 1. 构造节点列表
            var nodes = new List<NetworkNode>
            {
                new NetworkNode
                {
                    StationId = 1,
                    IpAddress = ConfigService.GetPlcIp(),
                    Port = ConfigService.GetPlcPort(),
                    Name = "主PLC"
                }
            };
            AllPositions = ConfigService.GetDevicePositions(null);
            // 2. 创建协调器（自动管理连接）
            _coordinator = new CommunicationCoordinator(nodes);

            // 3. 传递给TimechartService
            _timechartService = new TimechartService(_coordinator);

            _plcConn = _coordinator.GetPlcConnection(1);
            _deviceMonitor = new DeviceMonitor(_plcConn);
            _deviceMonitor.DeviceStatesUpdated += OnDeviceStatesUpdated;

            // 4. 启动设备监控
            await RobotManager.Instance.InitializeAsync();
            if (RobotManager.Instance.IsConnected)
                LogManager.Log("机器人已连接", LogLevel.Info, "Robot.Main");
            else
                LogManager.Log("机器人未连接", LogLevel.Warn, "Robot.Main");

            ///// 5. 初始化设备状态监控
            var deviceIndices = ConfigService.GetDeviceStations();
            var deviceCodes = deviceIndices.ToList();
            CbxFacility.DataSource = deviceCodes;
            CbxFacility.DisplayMember = "DeviceCode";   // 显示设备编号
            CbxFacility.ValueMember = "Index";   // 选中时取设备索引


            logBox = uiRichTextBox1;
            //tabControl1.SelectedIndex = 1;
            LogManager.Initialize(logBox);
            LogManager.Log("日志系统初始化完成", LogLevel.Info);
            uiButton1.BringToFront();


        }

        private void OnDeviceStatesUpdated(Dictionary<string, DeviceMonitor.DeviceState> deviceStates)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnDeviceStatesUpdated(deviceStates));
                return;
            }

            // 更新所有设备UI
            foreach (var (deviceCode, state) in deviceStates)
            {
                UpdateDeviceUI(deviceCode, state);
            }
        }

        private void UpdateDeviceUI(string deviceCode, DeviceMonitor.DeviceState state)
        {
            /* 暂时取消
            Color color = Color.Gray;

            if (state.Running) color = Color.Green;
            else if (state.Idle) color = Color.Yellow;
            else if (state.Paused) color = Color.Orange;
            else if (state.Fault) color = Color.Red;

            factoryLayoutControl1.SetDeviceBlockColor(deviceCode, color);

            // 显示报警状态
            factoryLayoutControl2.SetDeviceBlockColor(deviceCode, color);
            */

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

        private void but_stop_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 定点放片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_orientation_up_Click(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            var slot = cbx_Slot.Text;
            int deviceIndex = 0; // U1为0，U2为1，依次类推
            if (CbxFacility.Text == "")
            {
                MessageBox.Show("请先选择设备编号！");
                return;
            }
            string deviceCode = CbxFacility.Text; // 例如从界面选择
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
                MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
                return;
            }
            var plcConn = _coordinator.GetPlcConnection(1); // 1为主PLC站号

            if (plcConn == null)
            {
                MessageBox.Show("PLC连接未初始化！");
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor);

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
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void but_orientation_dow_Click(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            int deviceIndex = 0; // U1为0，U2为1，依次类推
            var slot = cbx_Slot.Text;
            if (CbxFacility.Text == "")
            {
                MessageBox.Show("请先选择设备编号！");
                return;
            }
            string deviceCode = CbxFacility.Text; // 例如从界面选择
            deviceIndex = ConfigService.GetDeviceIndex(deviceCode);
            if (deviceIndex < 0)
            {
                MessageBox.Show($"PLC 配置中未找到设备编号: {deviceCode}");
                return;
            }
            var plcConn = _coordinator.GetPlcConnection(1); // 1为主PLC站号
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
                MessageBox.Show("PLC连接未初始化！");
                return;
            }
            if (!plcConn.IsConnected)
                await plcConn.ConnectAsync();
            _executor = new SequenceTaskExecutor(plcConn, deviceIndex, slot, coord, _deviceMonitor);

            try
            {
                await _executor.GetRunAsync(_cts.Token);
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
    }
}
