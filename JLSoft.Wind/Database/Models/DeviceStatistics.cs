using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Models
{
    public class DeviceStatistics
    {
        public TimeSpan Uptime { get; set; }
        public int TasksCompleted { get; set; }
        public int Errors { get; set; }
        public int CommFailures { get; set; }
    }
}
