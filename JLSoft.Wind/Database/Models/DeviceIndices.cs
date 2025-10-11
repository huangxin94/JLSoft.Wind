using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JLSoft.Wind.Database.Models
{
    public class DeviceIndices
    {
        [JsonProperty("DeviceCode")]
        public string DeviceCode { get; set; }

        [JsonProperty("bieName")]
        public string bieName { get; set; }

        [JsonProperty("Index")]
        public int Index { get; set; }

        [JsonProperty("Positions")]
        public Positions Positions { get; set; }
        [JsonProperty("SubStations")]
        public List<SubStations> SubStations { get; set; }

    }
    /// <summary>
    /// 坐标
    /// </summary>
    public class Positions
    {
        [JsonProperty("X")]
        public double X { get; set; } = 0.0;

        [JsonProperty("Y")]
        public double Y { get; set; } = 0.0;

        [JsonProperty("Z")]
        public double Z { get; set; } = 0.0;
    }
    /// <summary>
    /// 子站点模型，包含设备索引列表。
    /// </summary>
    public class SubStations
    {
        [JsonProperty("Name")]
        public string Name { get; set; }


        [JsonProperty("bieName")]
        public string bieName { get; set; }


        [JsonProperty("Positions")]

        public Positions Positions { get; set; } = new Positions();
    }
}
