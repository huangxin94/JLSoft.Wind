using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.IServices;
using JLSoft.Wind.Logs;
using RobotSocketAPI;
using static JLSoft.Wind.Database.StationName;

namespace JLSoft.Wind.Services.Connect
{
    public class RobotManager : IRobotManager, IDisposable
    {
        private static readonly Lazy<RobotManager> _instance =
            new Lazy<RobotManager>(() => new RobotManager());
        public static RobotSocketClient _robot = new RobotSocketClient(ConfigService.GetRobotIp(), ConfigService.GetRobotPort());
        public static RobotManager Instance => _instance.Value;

        public bool IsConnected => _robotClient?.IsConnected == true;
        public IRobotSocketAPI RobotClient => _robotClient;

        private IRobotSocketAPI _robotClient; 
        private readonly string _ipAddress = ConfigService.GetRobotIp();
        private readonly int _port = ConfigService.GetRobotPort();
        private bool _disposed = false;
        private bool _initialized = false;

        // 私有构造函数确保单例
        private RobotManager() { }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                _robotClient = new RobotSocketClient(_ipAddress, _port);
                await _robotClient.ConnectAsync();
                _initialized = true;

                if (IsConnected)
                {
                    LogManager.Log("机器人连接成功", Sunny.UI.LogLevel.Info, "Robot.Main");
                }
                else
                {
                    LogManager.Log("机器人连接失败", Sunny.UI.LogLevel.Warn, "Robot.Main");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"机器人连接异常: {ex.Message}", Sunny.UI.LogLevel.Error, "Robot.Main");
                _initialized = true; // 标记已初始化，即使连接失败
            }
        }

        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            try
            {
                if (_robotClient == null)
                {
                    _robotClient = new RobotSocketClient(_ipAddress, _port);
                }


                if (IsConnected)
                {
                    LogManager.Log("机器人重新连接成功", Sunny.UI.LogLevel.Info, "Robot.Main");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"机器人重连异常: {ex.Message}", Sunny.UI.LogLevel.Error, "Robot.Main");
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _robotClient.Disconnect();
                LogManager.Log("机器人连接已断开", Sunny.UI.LogLevel.Info, "Robot.Main");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                _disposed = true;
            }
        }

        ~RobotManager()
        {
            Dispose(false);
        }


        public async Task Robot_GetAsync(Station station, string slot, CancellationToken cts)
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.GETAsync(sta, slot, cts);
        }

        public async Task Robot_PutAsync(Station station, string slot, CancellationToken cts)
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.PUTAsync(sta, slot, cts);
        }
        public async Task AxisMovStation(string station, Positions posit)
        {

            var stateX = Leisai_Axis.Leisai_Pmov(0, posit.X, 1);
            var stateZ = Leisai_Axis.Leisai_Pmov(2, posit.Z, 1);
            var stateY = Leisai_Axis.Leisai_Pmov(1, posit.Y, 1);
            await Task.WhenAll(stateY, stateZ);
        }
    }

}