using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Models.Timeline
{
    public class DeviceTimeline
    {
        /// <summary>
        /// 时间搓
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceStatus Status { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdate { get; set; }
        /// <summary>
        /// Cpu使用率
        /// </summary>
        public float CpuUsage { get; set; }
        /// <summary>
        /// 内存使用率
        /// </summary>
        public float MemoryUsage { get; set; }
        /// <summary>
        /// 任务队列长度
        /// </summary>
        public int TaskQueueLength { get; set; }
    }
}
