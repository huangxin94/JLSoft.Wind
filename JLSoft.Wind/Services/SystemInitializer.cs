using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JLSoft.Wind.Adapter;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services.Connect;
using JLSoft.Wind.Services.Status;
using JLSoft.Wind.UserControl;
using Sunny.UI;
using static JLSoft.Wind.Database.DB.ProductiveProcessMain;
using DeviceStatus = JLSoft.Wind.UserControl.DeviceStatus;

namespace JLSoft.Wind.Services
{
    public class SystemInitializer
    {
        public static bool IsInitialized { get; private set; }
        public bool IsRobotConnected { get; private set; }

        public bool IsPlcConnected { get; private set; }
        public static bool IsAlignerConnected { get; private set; }

        public static bool IsTeachServer { get; private set; }
        public static bool IsLoadPort1Connected { get; private set; }
        public static bool IsLoadPort2Connected { get; private set; }
        public static bool IsOCRAlignerConnected { get; private set; }
        public static bool IsOCRAngleTConnected { get; private set; }

        public event Action InitializationCompleted;

        private CommunicationCoordinator _coordinator;
        private DeviceMonitor _deviceMonitor;
        private TimechartService _timechartService;
        private PlcConnection _plcConn;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                LogManager.Log("系统初始化开始...", LogLevel.Info);

                // ========== 1. 初始化运动控制卡 ==========
                InitializeAxisControlCard();

                // ========== 2. 连接机器人 ==========
                IsRobotConnected = await InitializeRobotAsync();

                // ========== 3. 连接对齐器 ==========
                IsAlignerConnected = await InitializeAlignerAsync();


                _ = TeachServer.StartCmdServerAsync();
                _ = TeachServer.StartStatusServerAsync();

                // ========== 4. 连接加载端口 ==========
                await InitializeLoadPortsAsync();

                // ========== 5. 连接OCR设备 ==========
                await InitializeOcrDevicesAsync();

                // ========== 6. PLC通信初始化 ==========
                bool plcInitialized = false;
                if (ConfigService.IsPlcConnectionEnabled())
                {
                    _coordinator = await InitializePlcCoordinatorAsync();
                    IsPlcConnected = _plcConn?.IsConnected ?? false;

                    if (!IsPlcConnected && _plcConn != null)
                    {
                        await _plcConn.ConnectAsync();
                        IsPlcConnected = _plcConn.IsConnected;
                    }
                    plcInitialized = IsPlcConnected;
                }
                else
                {
                    LogManager.Log("PLC连接已禁用，跳过PLC初始化", LogLevel.Warn, "PLC");
                    IsPlcConnected = false;
                    plcInitialized = false;
                }

                // ========== 7. 创建设备监控服务 ==========
                if (_coordinator != null && plcInitialized)
                {
                    var pcl = _coordinator.GetPlcConnection(1);
                    _deviceMonitor = new DeviceMonitor(pcl);
                }
                else
                {
                    // PLC连接失败时，创建离线模式的DeviceMonitor
                    _deviceMonitor = CreateOfflineDeviceMonitor();
                }

                // ========== 8. 创建时间图表服务 ==========
                _timechartService = new TimechartService(_coordinator);

                // ========== 9. 检查所有设备状态 ==========
                IsInitialized = CheckAllDevicesConnected();

                LogManager.Log(IsInitialized ? "所有设备初始化成功" : "部分设备初始化失败",
                    IsInitialized ? LogLevel.Info : LogLevel.Error);

                InitializationCompleted?.Invoke();
                InitEvent.InitializeEvent();
                return IsInitialized;
            }
            catch (Exception ex)
            {
                LogManager.Log($"系统初始化失败: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        // 新增方法：强制更新所有设备UI状态
        private void UpdateAllDeviceUIStates()
        {
            try
            {
                if (_deviceMonitor != null)
                {
                    // 获取所有设备状态信息
                    var allStatusInfo = _deviceMonitor.GetAllDeviceStatusInfo();

                    foreach (var statusInfo in allStatusInfo)
                    {
                        var deviceCode = statusInfo.Key;
                        var status = statusInfo.Value;

                        // 根据状态设置设备UI
                        DeviceStatus uiStatus = ConvertToDeviceStatus(status);

                        // 更新两个工厂布局控件的设备状态
                        MainForm._instance?.factoryLayoutControl1.SetDeviceBlockBorderColor(deviceCode, uiStatus);
                        MainForm._instance?.factoryLayoutControl2.SetDeviceBlockBorderColor(deviceCode, uiStatus);
                    }
                }
                else
                {
                    // 设备监控为空时，设置所有设备为离线
                    SetAllDevicesToOffline();
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"更新设备UI状态失败: {ex.Message}", LogLevel.Error);
            }
        }

        // 新增方法：设置所有设备为离线状态
        private void SetAllDevicesToOffline()
        {
            try
            {
                var allDeviceCodes = new[] { "V1", "U1", "S3", "M1", "S4", "C1", "R1", "G1",
                                   "A1", "A2", "A3", "A4", "寻边机", "角度台", "LP1", "LP2" };

                foreach (var deviceCode in allDeviceCodes)
                {
                    MainForm._instance?.factoryLayoutControl1.SetDeviceBlockBorderColor(deviceCode, DeviceStatus.Offline);
                    MainForm._instance?.factoryLayoutControl2.SetDeviceBlockBorderColor(deviceCode, DeviceStatus.Offline);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"设置设备离线状态失败: {ex.Message}", LogLevel.Error);
            }
        }

        // 新增方法：将DeviceStatusInfo转换为DeviceStatus
        private DeviceStatus ConvertToDeviceStatus(DeviceMonitor.DeviceStatusInfo statusInfo)
        {
            if (!statusInfo.IsPlcConnected || statusInfo.IsOfflineMode)
                return DeviceStatus.Offline;

            if (statusInfo.DeviceState.Fault)
                return DeviceStatus.Fault;
            else if (statusInfo.DeviceState.Running)
                return DeviceStatus.Running;
            else if (statusInfo.DeviceState.Idle)
                return DeviceStatus.Idle;
            else if (statusInfo.DeviceState.Paused)
                return DeviceStatus.Paused;
            else
                return DeviceStatus.Offline;
        }
        #region 设备初始化方法
        private DeviceMonitor CreateOfflineDeviceMonitor()
        {
            try
            {
                // 创建一个模拟的PLC连接用于离线模式
                var offlinePlc = new PlcConnection(new NetworkNode
                {
                    StationId = 1,
                    IpAddress = "127.0.0.1",
                    Port = 8002,
                    Name = "离线模式"
                });

                var monitor = new DeviceMonitor(offlinePlc);
                // 手动设置为离线模式
                monitor.SetOfflineMode(true);
                return monitor;
            }
            catch (Exception ex)
            {
                LogManager.Log($"创建离线设备监控失败: {ex.Message}", LogLevel.Error);
                return null;
            }
        }
        private void InitializeAxisControlCard()
        {

            Leisai_Axis.Leisai_Init();

            // 设置各轴状态
            Leisai_Axis.SetAxisEnable(0); // X轴
            Leisai_Axis.SetAxisEnable(1); // Y轴
            Leisai_Axis.SetAxisEnable(2); // Z轴
        }

        private async Task<bool> InitializeRobotAsync()
        {
            try
            {
                //await RobotManager.Instance.InitializeAsync();
                //bool connected = RobotManager.Instance.IsConnected;
                var connected  = await HWRobot._robot.ConnectAsync();
                if (connected) {
                    await HWRobot.SGRPAsync("A"); //初始化类型和尺寸
                }
                //MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor(deviceCode, deviceStatus);
                LogManager.Log(connected ? "机器人已连接" : "机器人未连接",
                    connected ? LogLevel.Info : LogLevel.Warn, "Robot");
                return connected;
            }
            catch (Exception ex)
            {
                LogManager.Log($"机器人连接异常: {ex.Message}", LogLevel.Error, "Robot");
                return false;
            }
        }

        private async Task<bool> InitializeAlignerAsync()
        {
            try
            {
                bool connected = await HWAligner.Aligner_Connect();
                bool connectT = await AngleT.Connect();
                DeviceStatus deviceStatus = DeviceStatus.Idle;
                if (connected)
                {
                    deviceStatus = DeviceStatus.Idle;
                }
                else
                {
                    deviceStatus = DeviceStatus.Offline;
                }
                MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor("寻边机", deviceStatus);

                MainForm._instance.factoryLayoutControl2.SetDeviceBlockBorderColor("寻边机", deviceStatus);

                if (connectT)
                {
                    deviceStatus = DeviceStatus.Idle;
                }
                else
                {
                    deviceStatus = DeviceStatus.Offline;
                }
                MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor("角度台", deviceStatus);

                MainForm._instance.factoryLayoutControl2.SetDeviceBlockBorderColor("角度台", deviceStatus);
                LogManager.Log(connected ? "寻边机已连接" : "寻边机未连接",
                    connected ? LogLevel.Info : LogLevel.Warn, "Aligner");
                LogManager.Log(connectT ? "角度台已连接" : "角度台未连接",
                    connectT ? LogLevel.Info : LogLevel.Warn, "AngleT");
                return connected && connectT;
            }
            catch (Exception ex)
            {
                LogManager.Log($"寻边机、角度台连接异常: {ex.Message}", LogLevel.Error, "Aligner");
                return false;
            }
        }

        private async Task InitializeLoadPortsAsync()
        {
            try
            {
                IsLoadPort1Connected = await HWLoadPort_1.LoadPort1_Connect();
                IsLoadPort2Connected = await HWLoadPort_2.LoadPort2_Connect();
                DeviceStatus deviceStatus = DeviceStatus.Idle;
                if (IsLoadPort1Connected)
                {
                    deviceStatus = DeviceStatus.Idle;
                }
                else
                {
                    deviceStatus = DeviceStatus.Offline;
                }
                MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor("LP1", deviceStatus);
                MainForm._instance.factoryLayoutControl2.SetDeviceBlockBorderColor("LP1", deviceStatus);
                if (IsLoadPort2Connected)
                {
                    deviceStatus = DeviceStatus.Idle;
                }
                else
                {
                    deviceStatus = DeviceStatus.Offline;
                }

                MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor("LP2", deviceStatus);
                MainForm._instance.factoryLayoutControl2.SetDeviceBlockBorderColor("LP2", deviceStatus);
                LogManager.Log(IsLoadPort1Connected ? "加载端口1已连接" : "加载端口1未连接",
                    IsLoadPort1Connected ? LogLevel.Info : LogLevel.Warn, "LoadPort");

                LogManager.Log(IsLoadPort2Connected ? "加载端口2已连接" : "加载端口2未连接",
                    IsLoadPort2Connected ? LogLevel.Info : LogLevel.Warn, "LoadPort");
            }
            catch (Exception ex)
            {
                LogManager.Log($"加载端口连接异常: {ex.Message}", LogLevel.Error, "LoadPort");
            }
        }

        private async Task InitializeOcrDevicesAsync()
        {
            try
            {
                IsOCRAlignerConnected = await OCR_Aligner.OCR_Aligner_Connect();
                await OCR_Aligner.OCR_Aligner_InitAsync();
                IsOCRAngleTConnected = await OCR_AngleT.OCR_AngleT_Connect();
                await OCR_AngleT.OCR_AngleT_InitAsync();
                DeviceStatus deviceStatus = DeviceStatus.Idle;
                if (IsOCRAlignerConnected)
                {
                    deviceStatus = DeviceStatus.Idle;
                }
                else
                {
                    deviceStatus = DeviceStatus.Offline;
                }
                MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor("寻边器", deviceStatus);
                MainForm._instance.factoryLayoutControl2.SetDeviceBlockBorderColor("寻边器", deviceStatus);
                if (IsOCRAngleTConnected)
                {
                    deviceStatus = DeviceStatus.Idle;
                }
                else
                {
                    deviceStatus = DeviceStatus.Offline;
                }
                MainForm._instance.factoryLayoutControl1.SetDeviceBlockBorderColor("角度台", deviceStatus);
                MainForm._instance.factoryLayoutControl2.SetDeviceBlockBorderColor("角度台", deviceStatus);
                LogManager.Log(IsOCRAlignerConnected ? "对齐器OCR已连接" : "对齐器OCR未连接",
                    IsOCRAlignerConnected ? LogLevel.Info : LogLevel.Warn, "OCR");

                LogManager.Log(IsOCRAngleTConnected ? "角度T OCR已连接" : "角度T OCR未连接",
                    IsOCRAngleTConnected ? LogLevel.Info : LogLevel.Warn, "OCR");
            }
            catch (Exception ex)
            {
                LogManager.Log($"OCR设备连接异常: {ex.Message}", LogLevel.Error, "OCR");
            }
        }

        private async Task<CommunicationCoordinator> InitializePlcCoordinatorAsync()
        {
            try
            {
                // 创建网络节点
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

                // 创建通信协调器
                var coordinator = new CommunicationCoordinator(nodes);
                _plcConn = coordinator.GetPlcConnection(1);

                LogManager.Log("PLC通信协调器初始化完成", LogLevel.Info, "PLC");
                return coordinator;
            }
            catch (Exception ex)
            {
                LogManager.Log($"PLC通信协调器初始化异常: {ex.Message}", LogLevel.Error, "PLC");
                return null;
            }
        }

        #endregion

        private bool CheckAllDevicesConnected()
        {
            if (_plcConn != null)
            {
                return IsRobotConnected &&
                       IsAlignerConnected &&
                       IsLoadPort1Connected &&
                       IsLoadPort2Connected &&
                       IsOCRAlignerConnected &&
                       IsOCRAngleTConnected &&
                       _plcConn.IsConnected;
            }
            else
            {
                return IsRobotConnected &&
                       IsAlignerConnected &&
                       IsLoadPort1Connected &&
                       IsLoadPort2Connected &&
                       IsOCRAlignerConnected &&
                       IsOCRAngleTConnected;
            }
        }

        public List<DeviceIndices> GetDeviceStations()
        {
            return IsInitialized
                ? ConfigService.GetDeviceStations()
                : new List<DeviceIndices>();
        }

        public DeviceMonitor GetDeviceMonitor() => _deviceMonitor;
        public PlcConnection GetPlcConnection() => _plcConn;
    }
}