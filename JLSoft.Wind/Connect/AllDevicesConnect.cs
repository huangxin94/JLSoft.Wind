using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Connect
{
    public class AllDevicesConnect
    {

        public async void  ConnectAllDevices()
        {
            // 不需要特别配置线程池
            var deviceManager = new AsyncDeviceManager();

            // 添加7个设备
            deviceManager.AddDevice(1, "192.168.3.39", 6000);

            // 启动异步连接
            await deviceManager.ConnectAllAsync();

            Console.WriteLine("设备连接状态:");
            foreach (var (device, status) in deviceManager.GetConnectionStatus())
            {
                Console.WriteLine($"设备 {device.Id}: {status}");
            }


            //Console.WriteLine("按任意键关闭连接...");
            //Console.ReadKey();

            //await deviceManager.DisconnectAllAsync();
        }
    }
}
