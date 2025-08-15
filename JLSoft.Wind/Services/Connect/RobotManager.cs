using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.IServices;
using JLSoft.Wind.Logs;
using RobotSocketAPI;

namespace JLSoft.Wind.Services.Connect
{
    public class RobotManager : IRobotManager, IDisposable
    {
        private static readonly Lazy<RobotManager> _instance =
            new Lazy<RobotManager>(() => new RobotManager());

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
                await _robotClient.ConnectWithRetryAsync(3, 1000);
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

                await _robotClient.ConnectWithRetryAsync(3, 1000);

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
    }

}