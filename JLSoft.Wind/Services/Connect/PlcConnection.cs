using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunication;
using JLSoft.Wind.Database.Models;

namespace JLSoft.Wind.Services.Connect
{
    public class PlcConnection : IDisposable
    {
        public NetworkNode Node { get; }
        public MitsubishiPLC Plc { get; }
        public DateTime LastActive { get; private set; } = DateTime.MinValue;
        public bool IsConnected => Plc?.IsConnected ?? false;

        // 添加通信失败计数器
        public int CommunicationFailures { get;  set; }

        public PlcConnection(NetworkNode node)
        {
            Node = node;
            Plc = new MitsubishiPLC(node.IpAddress, node.Port);
        }

        // 尝试连接（返回 bool）
        public async Task<bool> ConnectAsync()
        {
            try
            {
                var result = await Plc.ConnectAsync();
                LastActive = DateTime.Now; // 无论成功失败都更新最后活动时间

                if (result.IsSuccess)
                {
                    CommunicationFailures = 0;
                    return true;
                }
                CommunicationFailures++;
                return false;
            }
            catch
            {
                LastActive = DateTime.Now;
                CommunicationFailures++;
                return false;
            }
        }
        // 断开连接
        public async Task DisconnectAsync()
        {
            if (Plc.IsConnected)
            {
                await Plc.DisconnectAsync();
            }
        }

        // 读取数据
        public async Task<OperateResult<short[]>> ReadDataAsync(string address, ushort length)
        {
            
            var result = await Plc.ReadDataAsync(address, length);
            if (result.IsSuccess) LastActive = DateTime.Now;
            return result;
        }

        public void Dispose()
        {
            DisconnectAsync().Wait();
            Plc?.Dispose();
        }
    }
}
