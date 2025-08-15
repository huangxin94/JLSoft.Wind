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

        private readonly PlcConnection _plcConnection;
        private readonly Timer _pollingTimer;
        private readonly Dictionary<string, DeviceState> _deviceStates;
        private readonly SemaphoreSlim _readSemaphore;
        private const int MaxWordsPerRead = 250; // 增大批量读取数量

        // 预计算地址缓存
        private readonly Dictionary<string, int[]> _deviceBitMappings;
        private readonly Dictionary<int, List<string>> _wordToDevices;
        private readonly HashSet<int> _allBitAddresses = new();
        public Dictionary<string, DeviceState> CurrentDeviceStates { get; private set; } = new();
        // 性能监控
        public long LastPollTimeMs { get; private set; }
        public int LastWordBlocksCount { get; private set; }
        public int LastTotalBitsRead { get; private set; }

        public event Action<Dictionary<string, DeviceState>> DeviceStatesUpdated;

        public DeviceMonitor(PlcConnection plcConnection, int pollingInterval = 300) // 增加轮询间隔
        {
            _plcConnection = plcConnection;

            // 根据处理器核心数设置信号量限制
            int maxConcurrency = Math.Max(2, Environment.ProcessorCount * 2);
            _readSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

            _deviceStates = new Dictionary<string, DeviceState>
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

            // 预计算所有地址映射
            _deviceBitMappings = new Dictionary<string, int[]>();
            _wordToDevices = new Dictionary<int, List<string>>();
            PrecomputeAddressMappings();

            _pollingTimer = new Timer(PollDeviceStates, null, 1000, pollingInterval); // 延迟启动
        }

        private void PrecomputeAddressMappings()
        {
            int index = 0;
            foreach (var deviceCode in _deviceStates.Keys)
            {
                // 计算每个设备的所有位地址
                int baseAddr = 0x0EC4 + index * 0x0340;
                int downstreamInlineAddr = 0x0D40 + index * 0x0340;
                int downstreamTroubleAddr = 0x0D41 + index * 0x0340;
                int upstreamInlineAddr = 0x0BC0 + index * 0x0340;
                int upstreamTroubleAddr = 0x0BC1 + index * 0x0340;

                // 每个设备需要8个位状态
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

                // 添加到总地址集合
                foreach (var addr in bitAddresses)
                {
                    _allBitAddresses.Add(addr);

                    // 填充 wordToDevices 字典
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

        private async void PollDeviceStates(object state)
        {
            var sw = Stopwatch.StartNew();


            try
            {
                // 3. 将位地址转换为字地址并合并连续块
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

                // 4. 执行批量读取并解析位状态
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
                            }
                            catch (Exception ex)
                            {
                                LogManager.Log($"PLC读取异常: {ex.Message}", LogLevel.Error, "PLC.Main");
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
            }
            catch (Exception ex)
            {
                LogManager.Log($"设备状态轮询异常: {ex.Message}", LogLevel.Error);
            }

            LastPollTimeMs = sw.ElapsedMilliseconds;
        }

        public DeviceState GetDeviceState(string deviceCode)
        {
            return _deviceStates.TryGetValue(deviceCode, out var state) ? state : null;
        }

        public void Dispose()
        {
            _pollingTimer?.Dispose();
            _readSemaphore?.Dispose();
        }
    }
}