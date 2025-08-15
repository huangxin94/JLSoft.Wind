using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;
using JLSoft.Wind.RobotControl;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 设备请求模型
    /// </summary>
    public class DeviceRequest
    {
        public int RequestId { get; set; } = Interlocked.Increment(ref _globalIdCounter);
        public RequestType Type { get; set; }
        public int SourceDevice { get; set; }
        public int? TargetDevice { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public object Payload { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);


        public Enum.Priority Priority { get; set; } = Priority.Normal;

        public DeviceRequest() 
        {

        }
        public DeviceRequest(RequestType type, int source, object payload, int? target = null)
        {
            Type = type;
            SourceDevice = source;
            Payload = payload;
            TargetDevice = target;
        }

        private static int _globalIdCounter;
    }
}
