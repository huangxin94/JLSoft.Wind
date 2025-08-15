using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 异步设备连接器，用于管理与设备的TCP连接。
    /// </summary>
    public class AsyncDeviceConnector
    {
        private TcpClient _client;
        public int Id { get; }
        public string IP { get; }
        public int Port { get; }
        public bool IsConnected => _client?.Connected == true;

        public AsyncDeviceConnector(int id, string ip, int port)
        {
            Id = id;
            IP = ip;
            Port = port;
        }

        public async Task ConnectAsync(int timeout = 3000)
        {
            try
            {
                _client = new TcpClient();

                // 创建异步连接任务
                var connectTask = _client.ConnectAsync(IP, Port);

                // 设置超时控制
                var timeoutTask = Task.Delay(timeout);
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"连接设备 {Id} 超时 ({timeout}ms)");
                }

                await connectTask; // 确保连接任务完成
                Console.WriteLine($"设备 {Id} 连接成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设备 {Id} 连接失败: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_client?.Connected == true)
            {
                await Task.Run(() => _client.Close()); // 异步关闭连接
                Console.WriteLine($"设备 {Id} 连接已关闭");
            }
        }
    }
}
