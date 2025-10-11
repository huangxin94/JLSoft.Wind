using JLSoft.Wind.Database.Models.Timeline;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Enum;
using JLSoft.Wind.RobotControl;
using System.Collections.Concurrent;
using JLSoft.Wind.Logs;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Sunny.UI;
using JLSoft.Wind.Services.Connect;
using JLSoft.Wind.Services;

namespace JLSoft.Wind.Adapter
{
    /// <summary>
    /// 通信协调器，负责管理与设备的通信、状态监控和请求处理。实现设备状态监控定时器
    /// </summary>
    public class CommunicationCoordinator : IDisposable
    {
        private readonly Dictionary<int, PlcConnection> _connections;
        private readonly Dictionary<int, DeviceStatusModel> _deviceStates;
        private readonly System.Timers.Timer _monitoringTimer;
        private bool _disposed;
        // 添加请求队列
        private readonly ConcurrentQueue<DeviceRequest> _requestQueue;

        // 请求锁
        private readonly object _queueLock = new object();

        // 请求处理定时器
        private readonly System.Timers.Timer _requestProcessor;
        public event Action<Dictionary<int, DeviceTimeline>> TimechartUpdated;

        public PlcConnection GetPlcConnection(int stationId)
        {
            if (_connections.TryGetValue(stationId, out var conn))
                return conn;
            return null;
        }
        private void OnDeviceStatusUpdated(int stationId, DeviceTimeline timeline)
        {
            var timelines = new Dictionary<int, DeviceTimeline> { { stationId, timeline } };
            TimechartUpdated?.Invoke(timelines);
        }
        public CommunicationCoordinator(IEnumerable<NetworkNode> nodes)
        {
            if (!ConfigService.IsPlcConnectionEnabled())
            {
                LogManager.Log("PLC连接已禁用，跳过通信协调器初始化", LogLevel.Info);
                return;
            }
            // 初始化PLC连接
            _connections = nodes.ToDictionary(
                n => n.StationId,
                n => new PlcConnection(n));
            
            // 初始化设备状态
            _deviceStates = new Dictionary<int, DeviceStatusModel>();
            foreach (var node in nodes)
            {
                _deviceStates[node.StationId] = new DeviceStatusModel
                {
                    StationId = node.StationId,
                    Status = DeviceStatusPLC.Unknown,
                    LastHeartbeat = DateTime.Now
                };
            }
            foreach (var conn in _connections.Values)
            {
                _ = conn.ConnectAsync(); // fire and forget，或用Task.WhenAll等待
            }
            // 启动监控定时器 (5秒检测一次)
            _monitoringTimer = new System.Timers.Timer(5000);
            _monitoringTimer.Elapsed += CheckDeviceStatus;
            _monitoringTimer.Start();
        }
        public void QueueDeviceRequest(DeviceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            lock (_queueLock)
            {
                // 确保请求不重复
                if (!_requestQueue.Any(r => r.RequestId == request.RequestId))
                {
                    _requestQueue.Enqueue(request);

                    // 调试信息（最终版本应移除）
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 添加请求 #{request.RequestId}");
                }
            }
        }
        /// <summary>
        /// 处理设备请求的定时器，定期检查并处理请求队列中的请求。实时读取PLC状态寄存器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckDeviceStatus(object sender, ElapsedEventArgs e)
        {
            foreach (var (stationId, state) in _deviceStates)
            {
                if (!_connections.TryGetValue(stationId, out var connection))
                    continue;

                try
                {
                    // 尝试读取状态位作为心跳检测
                    var result = await connection.ReadDataAsync("D0", 1);

                    if (result.IsSuccess)
                    {
                        state.Status = DeviceStatusPLC.Online;
                        state.LastHeartbeat = DateTime.Now;
                        state.Statistics.CommFailures = 0; // 重置设备统计中的失败计数
                        connection.CommunicationFailures = 0; // 重置连接失败计数
                    }
                    else
                    {
                        // 使用新添加的警告处理方法
                        HandleDeviceWarning(stationId, $"通信失败: {result.Message}");
                        state.Statistics.CommFailures++; // 更新设备统计
                        connection.CommunicationFailures++; // 更新连接失败计数
                    }
                }
                catch (Exception ex)
                {
                    // 使用新添加的错误处理方法
                    HandleDeviceError(stationId, $"通信异常: {ex.Message}");
                    state.Statistics.CommFailures++;
                    connection.CommunicationFailures++;
                }

                // 检查设备状态是否需要更新为离线
                if (state.Statistics.CommFailures >= 5)
                {
                    HandleDeviceOffline(stationId);
                }
            }
        }

        private void HandleDeviceWarning(int stationId, string message)
        {
            if (!_deviceStates.TryGetValue(stationId, out var state))
                return;

            // 更新设备状态为警告
            state.Status = DeviceStatusPLC.Warning;

            // 记录日志
            LogManager.Log($"PLC ⚠️ 设备警告: 站号={stationId}, 原因: {message}",LogLevel.Warn, "PLC.Main");

            // 添加到事件队列
            var deviceEvent = new DeviceEvent
            { 
                Type = EventType.Warning,
                Description = message,

            };

            // 确保队列不超过10个事件
            if (state.EventLog.Count >= 10)
            {
                state.EventLog.Dequeue();
            }
            state.EventLog.Enqueue(deviceEvent);
        }

        private void HandleDeviceError(int stationId, string message)
        {
            if (!_deviceStates.TryGetValue(stationId, out var state))
                return;

            // 更新设备状态为错误
            state.Status = DeviceStatusPLC.Error;

            // 记录日志
            LogManager.Log($"PLC ❌ 设备错误: 站号={stationId}, 原因: {message}", LogLevel.Error, "PLC.Main");

            // 添加到事件队列
            var deviceEvent = new DeviceEvent
            {
                Type = EventType.Error,
                Description = message,
                Timestamp = DateTime.Now
            };

            // 确保队列不超过10个事件
            if (state.EventLog.Count >= 10)
            {
                state.EventLog.Dequeue();
            }
            state.EventLog.Enqueue(deviceEvent);
        }

        // 设备离线处理（完整实现）
        private void HandleDeviceOffline(int stationId)
        {
            if (!_deviceStates.TryGetValue(stationId, out var state))
                return;

            // 更新状态
            state.Status = DeviceStatusPLC.Offline;

            // 日志记录
            LogManager.Log($"PLC ⚠️ 设备离线: 站号={stationId}, 名称={_connections[stationId].Node.Name}", LogLevel.Error, "PLC.Main");

            // 通知主站（Robot）
            NotifyMasterAboutOffline(stationId);

            // 尝试自动重连
            TryReconnectDevice(stationId);
        }

        // 通知主站设备离线
        private async void NotifyMasterAboutOffline(int stationId)
        {
            if (!_connections.TryGetValue(1, out var masterConnection) || !masterConnection.IsConnected)
                return;

            // 设置主站PLC的离线标志位（例如：M寄存器）
            // 例如：站号2离线 -> 设置 M200 = 1
            await masterConnection.Plc.WriteDataAsync($"M{200 + stationId}", 1);
        }

        // 尝试重连设备（修复后的版本）
        private async void TryReconnectDevice(int stationId)
        {
            if (!_connections.TryGetValue(stationId, out var connection))
                return;

            try
            {
                // 1. 断开现有连接
                await connection.DisconnectAsync();

                // 2. 延迟后重试
                await Task.Delay(3000);

                // 3. 尝试重新连接
                bool success = await connection.ConnectAsync();

                if (success)
                {
                    // 4. 更新状态
                    _deviceStates[stationId].Status = DeviceStatusPLC.Online;
                    _deviceStates[stationId].CommunicationFailures = 0;
                    LogManager.Log($"PLC ✅ 设备已重新连接: 站号={stationId}", LogLevel.Info, "PLC.Main");

                    // 5. 通知主站恢复在线状态
                    await NotifyMasterAboutRecovery(stationId);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"PLC ❌ 重连失败 [站号={stationId}]: {ex.Message}", LogLevel.Warn, "PLC.Main");
            }
        }

        // 通知主站设备恢复
        private async Task NotifyMasterAboutRecovery(int stationId)
        {
            if (!_connections.TryGetValue(1, out var masterConnection) || !masterConnection.IsConnected)
                return;

            // 清除主站PLC的离线标志位
            await masterConnection.Plc.WriteDataAsync($"M{200 + stationId}", 0);
        }

        // 获取设备状态
        public DeviceStatusModel GetDeviceState(int stationId)
        {
            return _deviceStates.TryGetValue(stationId, out var state)
                ? state
                : null;
        }

        // 获取所有设备状态
        public IReadOnlyDictionary<int, DeviceStatusModel> GetAllDeviceStates()
        {
            return _deviceStates;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _monitoringTimer.Stop();
            _monitoringTimer.Dispose();

            foreach (var connection in _connections.Values)
            {
                connection.Dispose();
            }

            _disposed = true;
        }
    }
}