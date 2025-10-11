using System;
using System.Threading.Tasks;
using HslCommunication.Profinet.Melsec;
using HslCommunication;
using JLSoft.Wind.Logs;

namespace JLSoft.Wind.Services.Connect
{
    public class MitsubishiPLC : IDisposable
    {
        // 单例实现
        private static readonly Lazy<MitsubishiPLC> _instance =
            new Lazy<MitsubishiPLC>(() => new MitsubishiPLC());
        public static MitsubishiPLC Instance => _instance.Value;

        // PLC连接对象
        private MelsecMcNet plcClient;
        public bool IsConnected { get; private set; } = false;
        private readonly object _lock = new object();

        // 配置参数
        private readonly string _ip;
        private readonly int _port;
        private bool _disposed = false;

        private MitsubishiPLC()
        {
            // 从配置获取连接参数
            _ip = ConfigService.GetPlcIp();
            _port = ConfigService.GetPlcPort();

            InitializeClient();
        }

        public MitsubishiPLC(string ip, int port)
        {
            // 从配置获取连接参数
            _ip = ip;
            _port = port;

            InitializeClient();
        }

        // 初始化客户端（不自动连接）
        private void InitializeClient()
        {
            try
            {
                plcClient = new MelsecMcNet(_ip, _port);
                plcClient.ConnectTimeOut = 2000;
                LogManager.Log("PLC客户端初始化完成",Sunny.UI.LogLevel.Info, "PLC.Main");
            }
            catch (Exception ex)
            {
                LogManager.Log($"PLC初始化异常: {ex.Message}",Sunny.UI.LogLevel.Warn, "PLC.Main");
            }
        }

        // 异步连接
        public async Task<OperateResult> ConnectAsync()
        {
            try
            {
                if (IsConnected)
                    return OperateResult.CreateSuccessResult();

                return await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        var result = plcClient.ConnectServer();
                        IsConnected = result.IsSuccess;

                        LogManager.Log(result.IsSuccess ?
                            "PLC连接成功" : $"PLC连接失败: {result.Message}", result.IsSuccess ? Sunny.UI.LogLevel.Info : Sunny.UI.LogLevel.Warn, "PLC.Main");

                        return result;
                    }
                });
            }
            catch (Exception ex)
            {
                LogManager.Log($"PLC连接异常: {ex.Message}", Sunny.UI.LogLevel.Warn, "PLC.Main");
                return new OperateResult(ex.Message);
            }
        }

        // 异步重连
        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await ConnectAsync();
        }

        // 异步断开
        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        plcClient?.ConnectClose();
                        IsConnected = false;
                        LogManager.Log("PLC连接已断开", Sunny.UI.LogLevel.Info, "PLC.Main");
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log($"PLC断开异常: {ex.Message}", Sunny.UI.LogLevel.Warn, "PLC.Main");
                    }
                }
            });
        }

        // 异步读取数据
        public async Task<OperateResult<short[]>> ReadDataAsync(string address, ushort length)
        {
            if (!IsConnected)
                return new OperateResult<short[]>("PLC未连接");

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        return plcClient.ReadInt16(address, length);
                    }
                    catch (Exception ex)
                    {
                        return new OperateResult<short[]>(ex.Message);
                    }
                }
            });
        }

        // 异步写入单个值
        public async Task<OperateResult> WriteDataAsync(string address, short value)
        {
            if (!IsConnected)
                return new OperateResult("PLC未连接");

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        return plcClient.Write(address, value);
                    }
                    catch (Exception ex)
                    {
                        return new OperateResult(ex.Message);
                    }
                }
            });
        }
        /// <summary>
        /// 异步写入单个布尔值
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<OperateResult> WriteBitAsync(string address, bool value)
        {
            if (!IsConnected)
                return new OperateResult("PLC未连接");

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        return plcClient.Write(address, value);
                    }
                    catch (Exception ex)
                    {
                        return new OperateResult(ex.Message);
                    }
                }
            });
        }
        // 异步写入多个值
        public async Task<OperateResult> WriteDataAsync(string address, short[] values)
        {
            if (!IsConnected)
                return new OperateResult("PLC未连接");

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        return plcClient.Write(address, values);
                    }
                    catch (Exception ex)
                    {
                        return new OperateResult(ex.Message);
                    }
                }
            });
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
                    DisconnectAsync().Wait(); // 同步等待断开连接
                }
                _disposed = true;
            }
        }

        ~MitsubishiPLC()
        {
            Dispose(false);
        }
    }
}