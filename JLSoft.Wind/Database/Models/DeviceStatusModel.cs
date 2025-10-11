using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Models.Timeline;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 设备状态模型，表示设备的当前状态、时间线信息和相关统计
    /// </summary>
    public class DeviceStatusModel
    {
        // 来自 DeviceState
        /// <summary>
        /// 设备所在的工作站ID
        /// </summary>
        public int StationId { get; set; }
        /// <summary>
        /// 设备当前状态
        /// </summary>
        public DeviceStatusPLC Status { get; set; }
        /// <summary>
        /// 设备最后一次心跳时间
        /// </summary>
        public DateTime LastHeartbeat { get; set; }
        /// <summary>
        /// 设备当前待处理请求数量
        /// </summary>
        public int PendingRequests { get; set; }
        /// <summary>
        /// 设备通信失败次数
        /// </summary>
        public int CommunicationFailures { get; set; }
        /// <summary>
        /// 设备统计信息，包括运行时间等
        /// </summary>

        // 来自 DeviceState
        public DeviceStatistics Statistics { get; } = new DeviceStatistics();
        /// <summary>
        /// 设备最近的事件队列，最多保留50个事件
        /// </summary>

        // 合并 DeviceState 的 RecentEvents 和 DeviceTimeline 的 EventLog
        public Queue<DeviceEvent> EventLog { get; } = new Queue<DeviceEvent>(50); // 保留最近50个事件
        /// <summary>
        /// 设备的时间线指标，包含状态、请求数量、心跳时间等信息
        /// </summary>
        // 来自 DeviceTimeline
        public Dictionary<string, object> Metrics { get; } = new Dictionary<string, object>();
        /// <summary>
        /// 设备的最后更新时间
        /// </summary>
        public DateTime LastUpdate { get; set; } // 最后更新时间
        /// <summary>
        /// 获取设备状态的指标信息，包括状态、待处理请求数量、心跳时间和运行时间等
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetMetrics()
        {
            return new Dictionary<string, object>
            {
                ["status"] = (int)Status,
                ["requests_pending"] = PendingRequests,
                ["heartbeat_age"] = (DateTime.Now - LastHeartbeat).TotalSeconds,
                ["uptime"] = Statistics.Uptime.TotalHours
            };
        }
    }
}
