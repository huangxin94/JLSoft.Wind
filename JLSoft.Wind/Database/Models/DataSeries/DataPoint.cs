using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Models.DataSeries
{
    public class DataPoint
    {
        public DateTime Timestamp { get; }
        public object Value { get; }

        public DataPoint(DateTime timestamp, object value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        public double NumericValue => Convert.ToDouble(Value);
    }
}
