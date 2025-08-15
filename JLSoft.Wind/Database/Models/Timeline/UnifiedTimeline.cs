using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Models.Timeline
{
    /// <summary>
    /// 统一时间线模型
    /// </summary>
    public class UnifiedTimeline
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public Dictionary<int, DeviceTimeline> Devices { get; } = new();

        public void UpdateDeviceState(int deviceId, DeviceStatus status)
        {
            if (!Devices.ContainsKey(deviceId))
            {
                Devices[deviceId] = new DeviceTimeline();
            }

            Devices[deviceId].Status = status;
            Devices[deviceId].LastUpdate = Timestamp;
        }
    }



}
