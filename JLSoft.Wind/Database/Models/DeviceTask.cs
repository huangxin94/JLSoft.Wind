using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;
using TaskStatus = JLSoft.Wind.Enum.TaskStatus;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 设备任务模型，表示对设备执行的具体操作任务。
    /// </summary>
    public class DeviceTask
    {
        /// <summary>
        /// 任务唯一标识符
        /// </summary>
        public Guid TaskId { get; } = Guid.NewGuid();
        /// <summary>
        /// 设备ID，表示该任务关联的设备
        /// </summary>
        public bool RequiresRobot { get; set; }
        /// <summary>
        /// 任务类型，表示任务的具体操作类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 任务目标设备ID，表示该任务要操作的目标设备
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
        
        public int Priority { get; set; }
        public List<string> RequiredTools { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public string RejectionReason { get; set; }
        public Action FallbackAction { get; set; }

        public DeviceTask()
        {
            Parameters = new Dictionary<string, object>();
            RequiredTools = new List<string>();
        }
    }
}
