using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sunny.UI; 

namespace JLSoft.Wind.Logs
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }

        public LogEntry() { }
        public LogEntry(DateTime date, string message, LogLevel level)
        {
            Timestamp = date;
            Message = message;
            Level = level;
        }
    }
}
