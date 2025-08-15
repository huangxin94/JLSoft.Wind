using System;
using System.Collections.Generic;
using JLSoft.Wind.Services;
using JLSoft.Wind.Services.Connect;

namespace JLSoft.Wind.Database.Struct
{
    public class DeviceMonitorManager : IDisposable
    {
        private readonly DevicePollingService _pollingService;

        public event Action<Dictionary<string, int>> DeviceStatesChanged;
        public event Action<Dictionary<string, short>> SpecialPointsChanged;

        public DeviceMonitorManager(PlcConnection plcConnection)
        {
            // 这里集中管理所有设备的地址映射
            var deviceAddresses = new List<DeviceStatusAddress>
            {
                new DeviceStatusAddress { DeviceCode = "V1", PlcAddresses = new[] { 0x0EC4, 0x0EC5, 0x0EC6, 0x0EC7 } },
                new DeviceStatusAddress { DeviceCode = "U1", PlcAddresses = new[] { 0x1204, 0x1205, 0x1206, 0x1207 } },
                new DeviceStatusAddress { DeviceCode = "S3", PlcAddresses = new[] { 0x1544, 0x1545, 0x1546, 0x1547 } },
                new DeviceStatusAddress { DeviceCode = "M1", PlcAddresses = new[] { 0x1884, 0x1885, 0x1886, 0x1887 } },
                new DeviceStatusAddress { DeviceCode = "S4", PlcAddresses = new[] { 0x1BC4, 0x1BC5, 0x1BC6, 0x1BC7 } },
                new DeviceStatusAddress { DeviceCode = "C1", PlcAddresses = new[] { 0x1F04, 0x1F05, 0x1F06, 0x1F07 } },
                new DeviceStatusAddress { DeviceCode = "R1", PlcAddresses = new[] { 0x2244, 0x2245, 0x2246, 0x2247 } },
                new DeviceStatusAddress { DeviceCode = "G1", PlcAddresses = new[] { 0x2584, 0x2585, 0x2586, 0x2587 } },
                new DeviceStatusAddress { DeviceCode = "A1", PlcAddresses = new[] { 0x28C4, 0x28C5, 0x28C6, 0x28C7 } },
                new DeviceStatusAddress { DeviceCode = "A2", PlcAddresses = new[] { 0x2C04, 0x2C05, 0x2C06, 0x2C07 } },
                new DeviceStatusAddress { DeviceCode = "A3", PlcAddresses = new[] { 0x2F44, 0x2F45, 0x2F46, 0x2F47 } },
                new DeviceStatusAddress { DeviceCode = "A4", PlcAddresses = new[] { 0x3284, 0x3285, 0x3286, 0x3287 } }
            };

            var specialPoints = new List<SpecialPointAddress>
            {
                new SpecialPointAddress { Name = "GlobalAlarm", PlcAddress = 0x4000 },
                new SpecialPointAddress { Name = "Interlock", PlcAddress = 0x4001 }
            };

            _pollingService = new DevicePollingService(plcConnection, deviceAddresses, specialPoints);
            _pollingService.DeviceStatesChanged += states => DeviceStatesChanged?.Invoke(states);
            _pollingService.SpecialPointsChanged += points => SpecialPointsChanged?.Invoke(points);
        }

        public void Dispose()
        {
            _pollingService?.Dispose();
        }
    }
}