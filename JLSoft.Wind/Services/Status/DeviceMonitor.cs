using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using JLSoft.Wind.Logs;
using Sunny.UI;
using Timer = System.Threading.Timer;
using JLSoft.Wind.Services.Connect;

namespace JLSoft.Wind.Services.Status
{
    public class DeviceMonitor : IDisposable
    {
        public class DeviceState
        {
            public string Code { get; }
            public bool Running { get; private set; }
            public bool Idle { get; private set; }
            public bool Paused { get; private set; }
            public bool Fault { get; private set; }
            public bool DownstreamInline { get; private set; }
            public bool DownstreamTrouble { get; private set; }
            public bool UpstreamInline { get; private set; }
            public bool UpstreamTrouble { get; private set; }

            private int _lastStateHash;

            public DeviceState(string code)
            {
                Code = code;
                _lastStateHash = 0;
            }

            public void UpdateFromPlcData(bool[] bitStates)
            {
                Running = bitStates[0];
                Idle = bitStates[1];
                Paused = bitStates[2];
                Fault = bitStates[3];
                DownstreamInline = bitStates[4];
                DownstreamTrouble = bitStates[5];
                UpstreamInline = bitStates[6];
                UpstreamTrouble = bitStates[7];
            }

            public int GetCurrentStateHash()
            {
                return HashCode.Combine(
                    Running, Idle, Paused, Fault,
                    DownstreamInline, DownstreamTrouble,
                    UpstreamInline, UpstreamTrouble);
            }

            public bool HasStateChanged()
            {
                int currentHash = GetCurrentStateHash();
                if (currentHash != _lastStateHash)
                {
                    _lastStateHash = currentHash;
                    return true;
                }
                return false;
            }
        }

        public class DeviceStatusInfo
        {
            public string DeviceCode { get; set; }
            public DeviceState DeviceState { get; set; }
            public bool IsPlcConnected { get; set; }
            public bool IsOfflineMode { get; set; }
            public DateTime LastUpdateTime { get; set; }
            public string StatusSummary => GetStatusSummary();

            private string GetStatusSummary()
            {
                if (!IsPlcConnected) return "PLC未连接";
                if (IsOfflineMode) return "离线模式";
                if (DeviceState.Fault) return "设备故障";
                if (DeviceState.Running) return "运行中";
                if (DeviceState.Idle) return "空闲";
                return "未知状态";
            }
        }

        // 原有字段
        private readonly PlcConnection _plcConnection;
        private readonly Timer _pollingTimer;
        private readonly Dictionary<string, DeviceState> _deviceStates;
        private readonly SemaphoreSlim _readSemaphore;
        private const int MaxWordsPerRead = 250;

        // 预计算地址缓存 - 在构造函数中初始化
        private readonly Dictionary<string, int[]> _deviceBitMappings;
        private readonly Dictionary<int, List<string>> _wordToDevices;
        private readonly HashSet<int> _allBitAddresses;
        public Dictionary<string, DeviceState> CurrentDeviceStates { get; private set; } = new Dictionary<string, DeviceState>();

        // 性能监控
        public long LastPollTimeMs { get; private set; }
        public int LastWordBlocksCount { get; private set; }
        public int LastTotalBitsRead { get; private set; }

        // 新增字段：离线模式支持
        private bool _isPlcConnected = false;
        private bool _isOfflineMode = false;
        private int _communicationFailureCount = 0;
        private readonly SemaphoreSlim _recoveryLock = new SemaphoreSlim(1, 1);
        private const int MaxCommunicationFailures = 3;

        // 新增属性
        public bool IsOfflineMode => _isOfflineMode;
        public bool IsPlcConnected => _isPlcConnected;
        public int CommunicationFailureCount => _communicationFailureCount;

        public event Action<Dictionary<string, DeviceState>> DeviceStatesUpdated;

        public DeviceMonitor(PlcConnection plcConnection, int pollingInterval = 300)
        {
            try
            {
                _plcConnection = plcConnection;

                // 确保在调用任何方法之前完成所有字段初始化
                _deviceBitMappings = new Dictionary<string, int[]>();
                _wordToDevices = new Dictionary<int, List<string>>();
                _allBitAddresses = new HashSet<int>();
                CurrentDeviceStates = new Dictionary<string, DeviceState>();

                // 检查PLC连接状态
                _isPlcConnected = _plcConnection?.IsConnected ?? false;

                // 根据处理器核心数设置信号量限制
                int maxConcurrency = Math.Max(2, Environment.ProcessorCount * 2);
                _readSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

                // 初始化设备状态
                _deviceStates = InitializeDeviceStates();

                // 添加空值检查
                if (_deviceBitMappings == null)
                {
                    _deviceBitMappings = new Dictionary<string, int[]>();
                    LogManager.Log("DeviceMonitor: _deviceBitMappings 被意外设置为null，重新初始化", LogLevel.Warn, "DeviceMonitor");
                }

                PrecomputeAddressMappings();

                if (_isPlcConnected)
                {
                    _pollingTimer = new Timer(PollDeviceStates, null, 1000, pollingInterval);
                    LogManager.Log("DeviceMonitor: PLC连接正常，启动状态轮询", LogLevel.Info, "DeviceMonitor");
                }
                else
                {
                    _isOfflineMode = true;
                    EnterOfflineMode();
                    _pollingTimer = new Timer(CheckPlcReconnection, null, 5000, 10000);
                    LogManager.Log("DeviceMonitor: PLC未连接，进入离线模式", LogLevel.Warn, "DeviceMonitor");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"DeviceMonitor构造函数异常: {ex.Message}", LogLevel.Error, "DeviceMonitor");
                // 即使出现异常，也确保基本功能可用
                _isOfflineMode = true;
                _deviceStates = InitializeDeviceStates();
            }
        }

        // 初始化设备状态
        private Dictionary<string, DeviceState> InitializeDeviceStates()
        {
            return new Dictionary<string, DeviceState>
            {
                ["V1"] = new DeviceState("V1"),
                ["U1"] = new DeviceState("U1"),
                ["S3"] = new DeviceState("S3"),
                ["M1"] = new DeviceState("M1"),
                ["S4"] = new DeviceState("S4"),
                ["C1"] = new DeviceState("C1"),
                ["R1"] = new DeviceState("R1"),
                ["G1"] = new DeviceState("G1"),
                ["A1"] = new DeviceState("A1"),
                ["A2"] = new DeviceState("A2"),
                ["A3"] = new DeviceState("A3"),
                ["A4"] = new DeviceState("A4")
            };
        }

        // 进入离线模式
        private void EnterOfflineMode()
        {
            try
            {
                SetAllDevicesToOfflineState();
                TriggerDeviceStateUpdate();
                LogManager.Log($"DeviceMonitor: 已设置{_deviceStates.Count}个设备为离线状态", LogLevel.Info, "DeviceMonitor");
            }
            catch (Exception ex)
            {
                LogManager.Log($"DeviceMonitor进入离线模式失败: {ex.Message}", LogLevel.Error, "DeviceMonitor");
            }
        }

        // 设置所有设备为离线状态
        private void SetAllDevicesToOfflineState()
        {
            foreach (var device in _deviceStates.Values)
            {
                bool[] offlineState = new bool[8]
                {
                    false,  // Running
                    false,  // Idle  
                    false,  // Paused
                    true,   // Fault (离线状态显示为故障)
                    false,  // DownstreamInline
                    false,  // DownstreamTrouble
                    false,  // UpstreamInline
                    false   // UpstreamTrouble
                };

                device.UpdateFromPlcData(offlineState);
            }
        }

        // 触发设备状态更新事件
        private void TriggerDeviceStateUpdate()
        {
            var currentStates = new Dictionary<string, DeviceState>();
            foreach (var kvp in _deviceStates)
            {
                currentStates[kvp.Key] = kvp.Value;

                // 同时更新CurrentDeviceStates
                if (!CurrentDeviceStates.ContainsKey(kvp.Key))
                {
                    CurrentDeviceStates[kvp.Key] = new DeviceState(kvp.Key);
                }
                CurrentDeviceStates[kvp.Key].UpdateFromPlcData(
                    new bool[] { kvp.Value.Running, kvp.Value.Idle, kvp.Value.Paused, kvp.Value.Fault,
                                kvp.Value.DownstreamInline, kvp.Value.DownstreamTrouble,
                                kvp.Value.UpstreamInline, kvp.Value.UpstreamTrouble });
            }

            DeviceStatesUpdated?.Invoke(currentStates);
        }

        // 检查PLC连接恢复
        private async void CheckPlcReconnection(object state)
        {
            // 使用异步方式获取锁
            if (await _recoveryLock.WaitAsync(0))
            {
                try
                {
                    if (_isOfflineMode && _plcConnection != null)
                    {
                        bool reconnected = await TryReconnectPlc();
                        if (reconnected)
                        {
                            RecoveryFromOfflineMode();
                        }
                    }
                }
                finally
                {
                    _recoveryLock.Release();
                }
            }
        }

        // 尝试重新连接PLC
        private async Task<bool> TryReconnectPlc()
        {
            try
            {
                if (_plcConnection.IsConnected)
                {
                    return true;
                }

                LogManager.Log("尝试重新连接PLC...", LogLevel.Info, "DeviceMonitor");
                await _plcConnection.ConnectAsync();

                if (_plcConnection.IsConnected)
                {
                    LogManager.Log("PLC重新连接成功", LogLevel.Info, "DeviceMonitor");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"PLC重新连接失败: {ex.Message}", LogLevel.Warn, "DeviceMonitor");
            }

            return false;
        }

        // 从离线模式恢复
        private async void RecoveryFromOfflineMode()
        {
            // 使用异步方式获取锁
            await _recoveryLock.WaitAsync();
            try
            {
                _isOfflineMode = false;
                _isPlcConnected = true;
                _communicationFailureCount = 0;

                LogManager.Log("DeviceMonitor: 从离线模式恢复，重新开始状态轮询", LogLevel.Info, "DeviceMonitor");

                // 立即触发一次状态刷新
                await ForceRefreshDeviceStates();
            }
            finally
            {
                _recoveryLock.Release();
            }
        }

        // 强制刷新设备状态
        private async Task ForceRefreshDeviceStates()
        {
            try
            {
                var tempState = new object();
                PollDeviceStates(tempState);
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                LogManager.Log($"强制刷新设备状态失败: {ex.Message}", LogLevel.Error, "DeviceMonitor");
            }
        }

        // 处理PLC通信失败
        private void HandlePlcCommunicationFailure()
        {
            _communicationFailureCount++;

            if (_communicationFailureCount >= MaxCommunicationFailures && !_isOfflineMode)
            {
                _isOfflineMode = true;
                EnterOfflineMode();
                LogManager.Log("PLC通信连续失败，进入离线模式", LogLevel.Error, "DeviceMonitor");
            }
        }

        // 预计算地址映射方法
        private void PrecomputeAddressMappings()
        {

            int index = 0;
            foreach (var deviceCode in _deviceStates.Keys)
            {
                int baseAddr = 0x0EC4 + index * 0x0340;
                int downstreamInlineAddr = 0x0D40 + index * 0x0340;
                int downstreamTroubleAddr = 0x0D41 + index * 0x0340;
                int upstreamInlineAddr = 0x0BC0 + index * 0x0340;
                int upstreamTroubleAddr = 0x0BC1 + index * 0x0340;

                int[] bitAddresses = new int[8]
                {
                    baseAddr,        // Running
                    baseAddr + 1,    // Idle
                    baseAddr + 2,    // Paused
                    baseAddr + 3,    // Fault
                    downstreamInlineAddr,    // DownstreamInline
                    downstreamTroubleAddr,   // DownstreamTrouble
                    upstreamInlineAddr,      // UpstreamInline
                    upstreamTroubleAddr      // UpstreamTrouble
                };

                _deviceBitMappings[deviceCode] = bitAddresses;

                foreach (var addr in bitAddresses)
                {
                    _allBitAddresses.Add(addr);
                    int wordAddr = addr / 16;
                    if (!_wordToDevices.TryGetValue(wordAddr, out var devices))
                    {
                        devices = new List<string>();
                        _wordToDevices[wordAddr] = devices;
                    }
                    if (!devices.Contains(deviceCode))
                    {
                        devices.Add(deviceCode);
                    }
                }

                index++;
            }
        }

        // 轮询方法
        private async void PollDeviceStates(object state)
        {
            // 如果处于离线模式但PLC已连接，尝试恢复
            if (_isOfflineMode && _plcConnection?.IsConnected == true)
            {
                // 使用异步方式获取锁
                await _recoveryLock.WaitAsync();
                try
                {
                    // 再次检查状态，因为可能在其他线程已经改变了
                    if (_isOfflineMode && _plcConnection?.IsConnected == true)
                    {
                        RecoveryFromOfflineMode();
                    }
                }
                finally
                {
                    _recoveryLock.Release();
                }
                return;
            }

            // 如果明确处于离线模式，不执行PLC读取
            if (_isOfflineMode) return;

            var sw = Stopwatch.StartNew();

            try
            {
                // 检查PLC连接状态
                if (!_plcConnection.IsConnected)
                {
                    throw new Exception("PLC连接已断开");
                }

                // 批量读取逻辑
                var wordAddresses = _allBitAddresses
                    .Select(bitAddr => bitAddr / 16)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                var wordBlocks = new List<(int startWord, int wordCount)>();
                if (wordAddresses.Any())
                {
                    int currentStart = wordAddresses[0];
                    int currentCount = 1;

                    for (int i = 1; i < wordAddresses.Count; i++)
                    {
                        if (wordAddresses[i] == wordAddresses[i - 1] + 1)
                        {
                            currentCount++;
                        }
                        else
                        {
                            wordBlocks.Add((currentStart, currentCount));
                            currentStart = wordAddresses[i];
                            currentCount = 1;
                        }
                    }
                    wordBlocks.Add((currentStart, currentCount));
                }

                LastWordBlocksCount = wordBlocks.Count;
                LastTotalBitsRead = _allBitAddresses.Count;

                var bitValueMap = new ConcurrentDictionary<int, bool>();
                var readTasks = new List<Task>();

                foreach (var block in wordBlocks)
                {
                    int remaining = block.wordCount;
                    int offset = 0;

                    while (remaining > 0)
                    {
                        int chunkSize = Math.Min(remaining, MaxWordsPerRead);
                        int currentStart = block.startWord + offset;
                        int plcReadStartAddr = currentStart * 16;

                        await _readSemaphore.WaitAsync();
                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                var result = await _plcConnection.ReadDataAsync(
                                    $"B{plcReadStartAddr:X4}", (ushort)chunkSize);

                                if (result.IsSuccess)
                                {
                                    for (int wordIndex = 0; wordIndex < result.Content.Length; wordIndex++)
                                    {
                                        int currentWordAddr = currentStart + wordIndex;
                                        short wordValue = result.Content[wordIndex];

                                        for (int bitIndex = 0; bitIndex < 16; bitIndex++)
                                        {
                                            int bitAddr = currentWordAddr * 16 + bitIndex;
                                            if (_allBitAddresses.Contains(bitAddr))
                                            {
                                                bool bitValue = (wordValue & 1 << bitIndex) != 0;
                                                bitValueMap[bitAddr] = bitValue;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                LogManager.Log($"PLC读取超时: B{plcReadStartAddr:X4}x{chunkSize}", LogLevel.Warn, "PLC.Main");
                                throw;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Log($"PLC读取异常: {ex.Message}", LogLevel.Error, "PLC.Main");
                                throw;
                            }
                            finally
                            {
                                _readSemaphore.Release();
                            }
                        });

                        readTasks.Add(task);
                        remaining -= chunkSize;
                        offset += chunkSize;
                    }
                }

                await Task.WhenAll(readTasks);

                // 更新设备状态
                foreach (var device in _deviceStates)
                {
                    var bitAddresses = _deviceBitMappings[device.Key];
                    bool[] bitStates = new bool[bitAddresses.Length];

                    for (int i = 0; i < bitAddresses.Length; i++)
                    {
                        bitStates[i] = bitValueMap.TryGetValue(bitAddresses[i], out var value) && value;
                    }

                    device.Value.UpdateFromPlcData(bitStates);

                    if (!CurrentDeviceStates.ContainsKey(device.Key))
                    {
                        CurrentDeviceStates[device.Key] = new DeviceState(device.Key);
                    }
                    CurrentDeviceStates[device.Key].UpdateFromPlcData(bitStates);
                }

                DeviceStatesUpdated?.Invoke(new Dictionary<string, DeviceState>(CurrentDeviceStates));

                // 重置通信失败计数
                _communicationFailureCount = 0;
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TimeoutException)
            {
                LogManager.Log($"PLC通信超时: {ex.Message}", LogLevel.Warn, "DeviceMonitor");
                HandlePlcCommunicationFailure();
            }
            catch (Exception ex)
            {
                LogManager.Log($"设备状态轮询异常: {ex.Message}", LogLevel.Error, "DeviceMonitor");
                HandlePlcCommunicationFailure();
            }
            finally
            {
                LastPollTimeMs = sw.ElapsedMilliseconds;
            }
        }

        public DeviceState GetDeviceState(string deviceCode)
        {
            return _deviceStates.TryGetValue(deviceCode, out var state) ? state : null;
        }

        public DeviceStatusInfo GetDeviceStatusInfo(string deviceCode)
        {
            var state = GetDeviceState(deviceCode);
            if (state == null)
            {
                state = new DeviceState(deviceCode);
                bool[] offlineData = new bool[8] { false, false, false, true, false, false, false, false };
                state.UpdateFromPlcData(offlineData);
            }

            return new DeviceStatusInfo
            {
                DeviceCode = deviceCode,
                DeviceState = state,
                IsPlcConnected = _isPlcConnected,
                IsOfflineMode = _isOfflineMode,
                LastUpdateTime = DateTime.Now
            };
        }

        public async void SetOfflineMode(bool offline)
        {
            _recoveryLock.Wait();
            try
            {
                if (offline && !_isOfflineMode)
                {
                    _isOfflineMode = true;
                    _isPlcConnected = false;
                    EnterOfflineMode();
                    LogManager.Log("DeviceMonitor: 手动设置为离线模式", LogLevel.Info, "DeviceMonitor");
                }
                else if (!offline && _isOfflineMode)
                {
                    _isOfflineMode = false;
                    LogManager.Log("DeviceMonitor: 手动退出离线模式", LogLevel.Info, "DeviceMonitor");
                }
            }
            finally
            {
                _recoveryLock.Release();
            }
        }

        public Dictionary<string, DeviceStatusInfo> GetAllDeviceStatusInfo()
        {
            var result = new Dictionary<string, DeviceStatusInfo>();
            foreach (var deviceCode in _deviceStates.Keys)
            {
                result[deviceCode] = GetDeviceStatusInfo(deviceCode);
            }
            return result;
        }

        public void Dispose()
        {
            _pollingTimer?.Dispose();
            _readSemaphore?.Dispose();
        }
    }
}