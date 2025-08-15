using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 传感器数据模型，表示从设备收集的传感器数据。
    /// </summary>
    public class SensorData
    {
        public int DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }
}
