using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Enum
{
    // 完全匹配图片中设备状态的实际类型
    public enum EquipmentStatus
    {
        Idle,          // 空闲（对应绿色）
        Processing,    // 加工中（对应黄色）
        Alarm,         // 报警（对应红色）
        Disconnected   // 断线（对应灰色）
    }

    // 与图片中Robot控件匹配的状态
    public enum RobotAxisStatus
    {
        Homing,        // 回零中
        Ready,         // 就绪
        Moving,        // 移动中
        EmergencyStop  // 急停状态
    }
}
