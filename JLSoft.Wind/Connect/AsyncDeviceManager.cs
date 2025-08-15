using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Models;

namespace JLSoft.Wind.Connect
{
    public class AsyncDeviceManager
    {
        private readonly List<AsyncDeviceConnector> _devices = new();

        public void AddDevice(int id, string ip, int port)
        {
            _devices.Add(new AsyncDeviceConnector(id, ip, port));
        }

        public async Task ConnectAllAsync()
        {
            Console.WriteLine("开始异步连接所有设备...");

            // 使用异步并行连接
            var connectTasks = new List<Task>();
            foreach (var device in _devices)
            {
                connectTasks.Add(device.ConnectAsync());
            }

            await Task.WhenAll(connectTasks);
        }

        public async Task DisconnectAllAsync()
        {
            var disconnectTasks = new List<Task>();
            foreach (var device in _devices)
            {
                disconnectTasks.Add(device.DisconnectAsync());
            }

            await Task.WhenAll(disconnectTasks);
        }

        public IEnumerable<(AsyncDeviceConnector device, string status)> GetConnectionStatus()
        {
            foreach (var device in _devices)
            {
                yield return (device, device.IsConnected ? "已连接" : "未连接");
            }
        }
    }
}
