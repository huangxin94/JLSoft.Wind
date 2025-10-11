using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 基本配置模型，包含机器人、PLC和用户信息。
    /// </summary>
    public class BasicConfiguration
    {

        [JsonProperty("Robot")]
        public RobotConfig Robot { get; set; }

        [JsonProperty("Slplc")]
        public PLCConfig Plc { get; set; }
        [JsonProperty("AngleTConfig")]
        public AngleTConfig AngleTConfig { get; set; }
        [JsonProperty("AlignerConfig")]
        public AlignerConfig AlignerConfig { get; set; }
        [JsonProperty("LoadPort1Config")]
        public string LoadPort1Config { get; set; }
        [JsonProperty("LoadPort2Config")]
        public string LoadPort2Config { get; set; }
        [JsonProperty("AlignerOCRConfig")]
        public PLCConfig AlignerOCRConfig { get; set; }
        [JsonProperty("AngleTOCRConfig")]
        public PLCConfig AngleTOCRConfig { get; set; }
        [JsonProperty("TeachServerConfig")]
        public PLCConfig TeachServerConfig { get; set; }


        [JsonProperty("DeviceIndices")]
        public List<DeviceIndices> Site { get; set; } = new();

        [JsonProperty("Users")]
        public List<UserData> Users { get; set; }
    }
}
