using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Database.Struct
{
    /// <summary>
    /// 设备地址结构体，表示设备内存的具体位置。
    /// </summary>
    public struct DeviceAddress
    {
        public MemoryType MemoryType { get; set; }
        public int ByteOffset { get; set; }
        public int BitIndex { get; set; }  // 位操作时使用

        public override string ToString()
        {
            return MemoryType switch
            {
                MemoryType.LB => $"LB{ByteOffset:X4}" + (BitIndex >= 0 ? $".{BitIndex}" : ""),
                MemoryType.LW => $"LW{ByteOffset:X4}",
                MemoryType.DB => $"DB{ByteOffset:X4}",
                _ => $"{MemoryType}_{ByteOffset:X4}"
            };
        }
    }

    public static class DeviceAddressHelper
    {
        public static string PutU1SignalAddress(string signal, int deviceIndex)
        {
            // 基础地址
            int baseAddr = signal switch
            {
                "ReceiveAble" => 0x0D4D,
                "ShutterState" => 0x0D59,
                "ReceiveStart" => 0x0D4E,
                "ReceiveComplete" => 0x0D4F,
                _ => throw new ArgumentException("未知信号")
            };
            return $"B{baseAddr + deviceIndex * 0x340:X}";
        }

        public static string PutRobotSignalAddress(string signal, int deviceIndex)
        {
            int baseAddr = signal switch
            {
                "SendAble" => 0x004D,
                "SendStart" => 0x004E,
                "SendComplete" => 0x004F,
                "JobTransferSignal" => 0x004C,
                _ => throw new ArgumentException("未知信号")
            };
            return $"B{baseAddr + deviceIndex * 0x40:X}";
        }


        public static string GetU1SignalAddress(string signal, int deviceIndex)
        {
            // 基础地址
            int baseAddr = signal switch
            {
                "SendAble" => 0x0BCD,
                "ShutterState" => 0x0BD9,
                "SendStart" => 0x0BCE,
                "SendComplete" => 0x0BCF,
                _ => throw new ArgumentException("未知信号")
            };
            return $"B{baseAddr + deviceIndex * 0x340:X}";
        }

        public static string GetRobotSignalAddress(string signal, int deviceIndex)
        {
            int baseAddr = signal switch
            {
                "ReceiveAble" => 0x040D,
                "ReceiveStart" => 0x040E,
                "ReceiveComplete" => 0x040F,
                "JobTransferSignal" => 0x040C,
                _ => throw new ArgumentException("未知信号")
            };
            return $"B{baseAddr + deviceIndex * 0x40:X}";
        }
    }
}
