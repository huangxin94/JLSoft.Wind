using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 机器人配置模型，表示机器人的网络配置信息。
    /// </summary>
    public class RobotConfig
    {
        [JsonProperty("IPAddress")]
        public string IpAddress { get; set; }

        [JsonProperty("Port")]
        public int Port { get; set; }
    }
}
