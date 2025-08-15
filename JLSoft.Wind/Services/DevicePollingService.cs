using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;
using JLSoft.Wind.Services.Connect;
using Timer = System.Threading.Timer;

namespace JLSoft.Wind.Services
{
    public class DevicePollingService : IDisposable
    {
        private readonly PlcConnection _plcConnection;
        private readonly List<DeviceStatusAddress> _deviceAddresses;
        private readonly List<SpecialPointAddress> _specialPoints;
        private readonly Timer _timer;
        private Dictionary<string, int> _lastStates = new();

        public event Action<Dictionary<string, int>> DeviceStatesChanged;
        public event Action<Dictionary<string, short>> SpecialPointsChanged;

        public DevicePollingService(
            PlcConnection plcConnection,
            List<DeviceStatusAddress> deviceAddresses,
            List<SpecialPointAddress> specialPoints)
        {
            _plcConnection = plcConnection;
            _deviceAddresses = deviceAddresses;
            _specialPoints = specialPoints;
            _timer = new Timer(PollAllDevices, null, 0, 1000);
        }

        private async void PollAllDevices(object state)
        {
            // 1. 合并所有地址
            var allAddrs = _deviceAddresses.SelectMany(d => d.PlcAddresses)
                .Concat(_specialPoints.Select(p => p.PlcAddress))
                .ToArray();
            int minAddr = allAddrs.Min();
            int maxAddr = allAddrs.Max();
            int count = maxAddr - minAddr + 1;

            var result = await _plcConnection.ReadDataAsync($"B{minAddr:X4}", (ushort)count);
            if (!result.IsSuccess) return;

            // 2. 解析每个设备的状态
            var deviceStates = new Dictionary<string, int>();
            foreach (var dev in _deviceAddresses)
            {
                int statusIndex = -1; // 改名，避免与参数冲突
                for (int i = 0; i < 4; i++)
                {
                    int idx = dev.PlcAddresses[i] - minAddr;
                    if (result.Content[idx] != 0)
                    {
                        statusIndex = i; // 0=Run, 1=Idle, 2=Pause, 3=BM
                        break;
                    }
                }
                deviceStates[dev.DeviceCode] = statusIndex;
            }

            // 3. 特殊点解析
            var specialStates = new Dictionary<string, short>();
            foreach (var sp in _specialPoints)
            {
                int idx = sp.PlcAddress - minAddr;
                specialStates[sp.Name] = result.Content[idx];
            }
            SpecialPointsChanged?.Invoke(specialStates);

            // 4. 状态有变化才通知
            if (HasChanged(deviceStates))
            {
                _lastStates = new Dictionary<string, int>(deviceStates);
                DeviceStatesChanged?.Invoke(deviceStates);
            }
        }

        private bool HasChanged(Dictionary<string, int> newStates)
        {
            if (_lastStates.Count != newStates.Count) return true;
            foreach (var kv in newStates)
            {
                if (!_lastStates.TryGetValue(kv.Key, out var oldVal) || oldVal != kv.Value)
                    return true;
            }
            return false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
