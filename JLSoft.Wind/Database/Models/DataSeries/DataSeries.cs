using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Models.DataSeries
{
    public class DataSeries
    {
        public string Name { get; }
        public DataType Type { get; }
        public int Capacity { get; }

        private readonly ConcurrentQueue<DataPoint> _points = new();

        public IReadOnlyCollection<DataPoint> Points => _points.ToArray();

        public DataSeries(string name, DataType type, int capacity = 1000)
        {
            Name = name;
            Type = type;
            Capacity = capacity;
        }

        public void AddPoint(DateTime timestamp, object value)
        {
            _points.Enqueue(new DataPoint(timestamp, value));

            // 维持容量大小
            while (_points.Count > Capacity && _points.TryDequeue(out _)) ;
        }

        public void Clear() => _points.Clear();
    }
}
