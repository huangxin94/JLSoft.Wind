using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Models.Timeline
{
    /// <summary>
    /// 设备事件模型，表示设备发生的特定事件及其相关信息。
    /// </summary>
    public class DeviceEvent
    {
        public DateTime Timestamp { get; set; }
        public EventType Type { get; set; }
        public string Description { get; set; }
        public object Details { get; set; }
        public DeviceEvent()
        {
            Timestamp = DateTime.Now; // 保持默认行为
        }
        public DeviceEvent(EventType type, string description, object details = null)
        {
            Timestamp = DateTime.Now;
            Type = type;
            Description = description;
            Details = details;
        }
    }
}
