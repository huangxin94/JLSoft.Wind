using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csIOC0640;

namespace JLSoft.Wind.Services
{
    public class LedControl
    {
        /// <summary>
        /// 上位机第一次启动状态
        /// </summary>
        public static void LedFirst()
        {
            IOC0640.ioc_write_outbit(0, 1, 0);
            IOC0640.ioc_write_outbit(0, 2, 1);
            IOC0640.ioc_write_outbit(0, 3, 1);
            IOC0640.ioc_write_outbit(0, 4, 0);
            IOC0640.ioc_write_outbit(0, 5, 1);
        }

        /// <summary>
        /// Home完成/正常待机
        /// </summary>
        public static void LedHomeFinish()
        {
            IOC0640.ioc_write_outbit(0, 1, 1);
            IOC0640.ioc_write_outbit(0, 2, 0);
            IOC0640.ioc_write_outbit(0, 3, 1);
            IOC0640.ioc_write_outbit(0, 4, 0);
            IOC0640.ioc_write_outbit(0, 5, 1);
        }

        /// <summary>
        /// 设备报警状态
        /// </summary>
        public static void LedAlarm()
        {
            IOC0640.ioc_write_outbit(0, 1, 0);
            IOC0640.ioc_write_outbit(0, 2, 1);
            IOC0640.ioc_write_outbit(0, 3, 1);
            IOC0640.ioc_write_outbit(0, 4, 0);
            IOC0640.ioc_write_outbit(0, 5, 0);
        }

        /// <summary>
        /// 设备运行状态
        /// </summary>
        public static void LedRun()
        {
            IOC0640.ioc_write_outbit(0, 1, 1);
            IOC0640.ioc_write_outbit(0, 2, 1);
            IOC0640.ioc_write_outbit(0, 3, 0);
            IOC0640.ioc_write_outbit(0, 4, 1);
            IOC0640.ioc_write_outbit(0, 5, 1);
        }
    }
}