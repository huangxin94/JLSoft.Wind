using System;

namespace JLSoft.Wind.Database.Struct
{
    /// <summary>
    /// 表示一个设备的状态地址映射（如S1的4个状态地址）。
    /// </summary>
    public class DeviceStatusAddress
    {
        /// <summary>
        /// 设备编号（如"S1"、"S2"等）
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// 该设备对应的4个连续PLC地址（如0x0EC4, 0x0EC5, 0x0EC6, 0x0EC7）
        /// </summary>
        public int[] PlcAddresses { get; set; }
    }
}