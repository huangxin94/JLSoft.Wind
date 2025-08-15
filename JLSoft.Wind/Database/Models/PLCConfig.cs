using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// PLC配置模型，表示PLC的网络配置信息。
    /// </summary>
    public class PLCConfig
    {
        [JsonProperty("IPAddress")]
        public string IpAddress { get; set; }

        [JsonProperty("Port")]
        public int Port { get; set; }
    }
}
