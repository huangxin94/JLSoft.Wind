using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JLSoft.Wind.Enum;
using TaskStatus = JLSoft.Wind.Enum.TaskStatus;

namespace JLSoft.Wind.RobotControl
{
    /// <summary>
    /// 表示机器人的任务模型，包含任务的基本信息和状态。
    /// </summary>
    public class RobotTask
    {

        public int DeviceId { get; set; } // 新增设备ID
        public List<string> RequiredTools { get; set; } = new(); // 新增所需工具列表
        public Guid TaskId { get; } = Guid.NewGuid();
        public int TargetDevice { get; set; }
        public string TaskType { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime CreatedAt { get; } = DateTime.Now;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public int Priority { get; set; } // 0=最低, 9=最高
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public bool IsCritical { get; set; } = false;

        public string OperationName => $"Task_{TaskType}_{TargetDevice}_{TaskId.ToString().Substring(0, 4)}";
    }
}
