using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Class.Models
{
    public class DeviceJobDataConfig
    {
        public string DeviceCode { get; set; }
        public ushort JobDataStartAddress { get; set; }

        // 其他配置项...
    }

    public static class DeviceConfigManager
    {
        private static readonly Dictionary<string, DeviceJobDataConfig> _deviceConfigs = new();
        private static readonly string[] _site = {"V1 Up","V1","U1", "S3", "M1", "S4","C1","R1","G1","A1","A2","A3","A4" };
        public static void LoadConfigurations()
        {
            // 从数据库或配置文件加载配置
            // 示例配置：
            var StartOffset = 0x0050; // 偏移量示例
            var i = 0;
            foreach (var deviceCode in _site)
            {

                if (!_deviceConfigs.ContainsKey(deviceCode))
                {
                    _deviceConfigs[deviceCode] = new DeviceJobDataConfig
                    {
                        DeviceCode = deviceCode,
                        JobDataStartAddress = (ushort)(0x0000 + i * StartOffset)  // 默认地址，可以根据实际情况调整
                    };
                }
            }
        }

        public static DeviceJobDataConfig GetDeviceConfig(string deviceCode)
        {
            return _deviceConfigs.TryGetValue(deviceCode, out var config) ? config : null;
        }
    }
}
