using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Enum
{
    /// <summary>
    /// 状态枚举，表示设备的不同状态。
    /// </summary>
    public enum DeviceStatusPLC
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        Unknown,
        /// <summary>
        /// 在线状态
        /// </summary>
        Online,
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idle,
        /// <summary>
        /// 正在请求主站
        /// </summary>
        Requesting,
        /// <summary>
        /// 正在处理任务
        /// </summary>
        Processing,
        /// <summary>
        /// 任务完成
        /// </summary>
        Completed,
        /// <summary>
        /// 离线状态
        /// </summary>
        Offline,
        /// <summary>
        /// 警告状态
        /// </summary>
        Warning,
        /// <summary>
        /// 错误状态
        /// </summary>
        Error
    }

    /// <summary>
    /// 通信模式枚举，表示设备与主站之间的通信方式。
    /// </summary>
    public enum CommunicationMode
    {
        /// <summary>
        /// 主站广播模式
        /// </summary>
        Broadcast,      // 
        /// <summary>
        /// 主站轮询模式
        /// </summary>
        DirectPolling,  // 
        /// <summary>
        /// 设备请求模式
        /// </summary>
        OnDemand        // 
    }

    /// <summary>
    /// 请求类型枚举，表示设备请求的类型。
    /// </summary>
    public enum MemoryType
    {
        /// <summary>
        /// 位存储区
        /// </summary>
        LB,
        /// <summary>
        /// 字存储区
        /// </summary>
        LW,
        /// <summary>
        /// 数据块存储区
        /// </summary>
        DB  
    }

    /// <summary>
    /// 任务状态枚举，表示设备任务的不同状态。
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending,
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        /// <summary>
        /// 失败
        /// </summary>
        Failed,
        /// <summary>
        /// 已取消/拒绝
        /// </summary>
        Rejected,
        /// <summary>
        /// 已取消
        /// </summary>
        Canceled
    }
    /// <summary>
    /// 设备请求类型
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// 检查机器人可用性
        /// </summary>
        RobotAvailabilityCheck,
        /// <summary>
        /// 分配任务
        /// </summary>
        TaskAssignment,
        /// <summary>
        /// 查询数据
        /// </summary>
        DataQuery,
        /// <summary>
        /// 紧急停止
        /// </summary>
        EmergencyStop,
        /// <summary>
        /// 控制命令（新增）
        /// </summary>
        ControlCommand,
        /// <summary>
        /// 更新设备状态
        /// </summary>
        DeviceStatusUpdate
    }
    /// <summary>
    /// 设备请求状态
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>
        /// 等待
        /// </summary>
        Pending,
        /// <summary>
        /// 处理中
        /// </summary>
        Processing,
        /// <summary>
        /// 完成
        /// </summary>
        Completed,
        /// <summary>
        /// 失败
        /// </summary>
        Failed,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout
    }
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 状态变更
        /// </summary>
        StatusChange
    }

    /// <summary>
    /// 数据类型枚举，表示设备数据的不同类型。
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 布尔类型
        /// </summary>
        Boolean,
        /// <summary>
        /// 整型
        /// </summary>
        Integer,
        /// <summary>
        /// 浮点型
        /// </summary>
        Float,
        /// <summary>
        /// 字符串类型
        /// </summary>
        String,
        /// <summary>
        /// 对象类型
        /// </summary>
        Object,
        /// <summary>
        /// 枚举类型
        /// </summary>
        Enum
    }
    /// <summary>
    /// 节点状态枚举，表示设备节点的状态。
    /// </summary>
    public enum NodeStatus
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 在线状态
        /// </summary>
        Online = 1,
        /// <summary>
        /// 离线状态
        /// </summary>
        Offline = 2,
        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 3
    }
    /// <summary>
    /// 优先级枚举，表示设备请求的优先级。
    /// </summary>
    public enum Priority
    {
        /// <summary>
        /// 低优先级
        /// </summary>
        Low,
        /// <summary>
        /// 正常优先级（之前缺失的关键定义）
        /// </summary>
        Normal,
        /// <summary>
        /// 高优先级
        /// </summary>
        High,
        /// <summary>
        /// 紧急优先级
        /// </summary>
        Critical
    }
}
