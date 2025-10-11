using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Services
{
    public class StoredProcedureResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DataTable ResultData { get; set; }
        public int ReturnValue { get; set; }
        public Dictionary<string, object> OutputParameters { get; set; }
        public DateTime ExecutionTime { get; set; }
        public TimeSpan ExecutionDuration { get; set; }

        public StoredProcedureResult()
        {
            OutputParameters = new Dictionary<string, object>();
            ExecutionTime = DateTime.Now;
            IsSuccess = false;
            Message = "未执行";
        }
    }
}
