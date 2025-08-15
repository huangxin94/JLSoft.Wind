using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Connect
{
    public class PLCCommunication
    {
        private TcpClient client;
        private NetworkStream stream;
        private readonly string plcIpAddress;
        private readonly int plcPort;
        private readonly int networkNumber;
        private readonly int pcNumber;
        private readonly int requestDestinationModuleIoNumber;
        private readonly int requestDestinationModuleStationNumber;

        // 构造函数，初始化PLC连接参数
        public PLCCommunication(string ipAddress, int port = 5007,
            int networkNumber = 0, int pcNumber = 0xFF,
            int destinationModuleIoNumber = 0x03FF, int destinationModuleStationNumber = 0)
        {
            plcIpAddress = ipAddress;
            plcPort = port;
            this.networkNumber = networkNumber;
            this.pcNumber = pcNumber;
            requestDestinationModuleIoNumber = destinationModuleIoNumber;
            requestDestinationModuleStationNumber = destinationModuleStationNumber;
        }

        // 连接到PLC
        public bool Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(IPAddress.Parse(plcIpAddress), plcPort);
                stream = client.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接错误: {ex.Message}");
                return false;
            }
        }

        // 断开连接
        public void Disconnect()
        {
            stream?.Close();
            client?.Close();
        }

        // 读取PLC数据
        public byte[] Read(string device, int address, int size)
        {
            try
            {
                byte[] request = BuildReadRequest(device, address, size);
                SendRequest(request);
                return ReceiveResponse(size * 2); // 每个数据点占2字节
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取错误: {ex.Message}");
                return null;
            }
        }

        // 写入PLC数据
        public bool Write(string device, int address, byte[] data)
        {
            try
            {
                byte[] request = BuildWriteRequest(device, address, data);
                SendRequest(request);
                byte[] response = ReceiveResponse(12); // 响应头长度
                return CheckResponseSuccess(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入错误: {ex.Message}");
                return false;
            }
        }

        // 构建读取请求
        private byte[] BuildReadRequest(string device, int address, int size)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            // 构建MC协议帧头
            writer.Write((short)0x5000); // 副标题
            writer.Write((short)0x00FF); // 网络编号
            writer.Write((byte)pcNumber); // PC编号
            writer.Write((short)requestDestinationModuleIoNumber); // 目标模块I/O编号
            writer.Write((byte)requestDestinationModuleStationNumber); // 目标模块站号
            writer.Write((short)(12 + 7)); // 请求数据长度

            // 构建指令部分
            writer.Write((short)0x0401); // 指令 - 读取
            writer.Write((short)0x0000); // 子指令

            // 构建设备地址信息
            writer.Write((byte)GetDeviceCode(device)); // 设备代码
            writer.Write(ConvertTo32BitAddress(address)); // 地址
            writer.Write((short)size); // 读取点数

            return ms.ToArray();
        }

        // 构建写入请求
        private byte[] BuildWriteRequest(string device, int address, byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            // 构建MC协议帧头
            writer.Write((short)0x5000); // 副标题
            writer.Write((short)0x00FF); // 网络编号
            writer.Write((byte)pcNumber); // PC编号
            writer.Write((short)requestDestinationModuleIoNumber); // 目标模块I/O编号
            writer.Write((byte)requestDestinationModuleStationNumber); // 目标模块站号
            writer.Write((short)(12 + 7 + data.Length)); // 请求数据长度

            // 构建指令部分
            writer.Write((short)0x1401); // 指令 - 写入
            writer.Write((short)0x0000); // 子指令

            // 构建设备地址信息
            writer.Write((byte)GetDeviceCode(device)); // 设备代码
            writer.Write(ConvertTo32BitAddress(address)); // 地址
            writer.Write((short)(data.Length / 2)); // 写入点数
            writer.Write(data); // 写入数据

            return ms.ToArray();
        }

        // 发送请求
        private void SendRequest(byte[] request)
        {
            stream.Write(request, 0, request.Length);
        }

        // 接收响应
        private byte[] ReceiveResponse(int expectedDataLength)
        {
            byte[] header = new byte[12];
            stream.Read(header, 0, 12);

            short responseLength = BitConverter.ToInt16(header, 10);
            byte[] data = new byte[responseLength - 2]; // 减去长度字段本身

            int bytesRead = 0;
            while (bytesRead < data.Length)
            {
                int read = stream.Read(data, bytesRead, data.Length - bytesRead);
                if (read == 0) throw new IOException("连接已关闭");
                bytesRead += read;
            }

            return CombineArrays(header, data);
        }

        // 检查响应是否成功
        private bool CheckResponseSuccess(byte[] response)
        {
            if (response == null || response.Length < 16) return false;

            short errorCode = BitConverter.ToInt16(response, 14);
            return errorCode == 0;
        }

        // 获取设备代码
        private int GetDeviceCode(string device)
        {
            switch (device.ToUpper())
            {
                case "D": return 0xA8; // 数据寄存器
                case "M": return 0x90; // 辅助继电器
                case "X": return 0x9C; // 输入继电器
                case "Y": return 0x9D; // 输出继电器
                case "T": return 0xA0; // 定时器触点
                case "TS": return 0xA1; // 定时器当前值
                case "C": return 0xA2; // 计数器触点
                case "CS": return 0xA3; // 计数器当前值
                default: throw new ArgumentException($"不支持的设备类型: {device}");
            }
        }

        // 转换为32位地址
        private byte[] ConvertTo32BitAddress(int address)
        {
            byte[] bytes = BitConverter.GetBytes((int)address);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        // 合并数组
        private byte[] CombineArrays(byte[] a1, byte[] a2)
        {
            byte[] result = new byte[a1.Length + a2.Length];
            Array.Copy(a1, 0, result, 0, a1.Length);
            Array.Copy(a2, 0, result, a1.Length, a2.Length);
            return result;
        }

    }
}
