using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Class.Models
{
    public class DeviceStatus
    {
        public bool Running { get; set; }
        public bool Idle { get; set; }
        public bool Paused { get; set; }
        public bool Fault { get; set; }
        public bool DownstreamInline { get; set; }
        public bool DownstreamTrouble { get; set; }
        public bool UpstreamInline { get; set; }
        public bool UpstreamTrouble { get; set; }

        // 添加业务判断方法
        public bool IsOperational => Running && !Fault;
        public bool NeedsAttention => Fault || DownstreamTrouble || UpstreamTrouble;
    }
}
