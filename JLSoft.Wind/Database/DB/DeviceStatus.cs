using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;
using SqlSugar;

namespace JLSoft.Wind.Database.DB
{
    [DbTable("device_status")]
    public class DeviceStatus
    {
        [DbColumn("device_id", IsPrimaryKey = true)]
        public int DeviceId { get; set; }

        [DbColumn]
        public string Status { get; set; }

        [DbColumn("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
