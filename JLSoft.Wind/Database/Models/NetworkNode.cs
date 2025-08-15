using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Models
{
    public class NetworkNode
    {
        public int StationId { get; set; }      // PLC站号 (1-13)
        public string IpAddress { get; set; }   // PLC IP地址
        public int Port { get; set; }           // PLC端口
        public string Name { get; set; }         // 设备名称
        public bool IsMaster { get; set; }       // 是否主站
    }
}
