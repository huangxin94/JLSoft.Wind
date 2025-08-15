using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;
using SqlSugar;

namespace JLSoft.Wind.Database.DB
{
    [DbTable("sensor_data")]
    public class SensorData
    {
        [DbColumn("device_id")]
        public int DeviceId { get; set; }

        [DbColumn]
        public DateTime Timestamp { get; set; }

        [DbColumn]
        public double Value { get; set; }
    }
}
