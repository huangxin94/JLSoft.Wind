using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Database.Struct;
using JLSoft.Wind.Enum;
using JLSoft.Wind.RobotControl;

namespace JLSoft.Wind.Adapter
{
    public class DeviceCommunicationAPI
    {
        /// <summary>
        /// 设备站点ID，用于标识当前设备在系统中的唯一性。
        /// </summary>
        private readonly int _stationId;
        /// <summary>
        /// 通信协调器，用于管理设备请求和响应的队列。
        /// </summary>
        private readonly CommunicationCoordinator _coordinator;
        /// <summary>
        /// 标准化地址映射，用于设备通信中的地址转换。
        /// </summary>
        private readonly Dictionary<string, DeviceAddress> _standardAddressMap;
        /// <summary>
        /// 最后一次响应，存储设备的最新响应信息。
        /// </summary>
        private RobotResponse _lastResponse;
        /// <summary>
        /// 线程安全的锁对象，用于保护对_lastResponse的访问。
        /// </summary>
        private readonly object _responseLock = new();
        /// <summary>
        /// 检查是否有响应可用。
        /// </summary>
        public bool HasResponse
        {
            get
            {
                lock (_responseLock)
                {
                    return _lastResponse != null;
                }
            }
        }
        /// <summary>
        /// 新增响应获取方法
        /// </summary>
        /// <returns></returns>
        public RobotResponse GetResponse()
        {
            lock (_responseLock)
            {
                var response = _lastResponse;
                _lastResponse = null; // 取走后清除
                return response;
            }
        }
        /// <summary>
        /// 添加设置响应的方法（供协调器调用）
        /// </summary>
        /// <param name="response"></param>
        public void SetResponse(RobotResponse response)
        {
            lock (_responseLock)
            {
                _lastResponse = response;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="coordinator"></param>
        public DeviceCommunicationAPI(int stationId, CommunicationCoordinator coordinator)
        {
            _stationId = stationId;
            _coordinator = coordinator;
            _standardAddressMap = LoadStandardAddressMap();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RequestRobotTask(RobotTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            var request = new DeviceRequest(
                type: RequestType.TaskAssignment,
                source: _stationId,
                payload: task,  // 将 task 存入 Payload
                target: null
            )
            {
                // 补充初始化器属性
                RequestId = _stationId,
                Priority = task.IsCritical ? Priority.High : Priority.Normal
            };
            _coordinator.QueueDeviceRequest(request);
        }
        /// <summary>
        /// 向设备发送状态更新请求
        /// </summary>
        public void SignalCompletion()
        {
            var request = new DeviceRequest
            {
                Type = RequestType.DeviceStatusUpdate,
                RequestId = _stationId,
                Status = RequestStatus.Completed
            };

            _coordinator.QueueDeviceRequest(request);
        }

        public void EmergencyStop()
        {
            var request = new DeviceRequest(
                type: RequestType.EmergencyStop,
                source: _stationId, // 明确来源设备
                payload: null,
                target: null)
            {
                RequestId = _stationId,
                Priority = Priority.Critical
            };
            _coordinator.QueueDeviceRequest(request);
        }

        private static Dictionary<string, DeviceAddress> LoadStandardAddressMap()
        {
            // 加载标准化地址映射
            return new Dictionary<string, DeviceAddress>
            {
                // Robot相关地址
                ["Robot.IdleStatus"] = new() { MemoryType = MemoryType.LB, ByteOffset = 0x0780 },
                ["Robot.CurrentTask"] = new() { MemoryType = MemoryType.LW, ByteOffset = 0x004D0 },

                // 通用状态地址
                ["Device.Status"] = new() { MemoryType = MemoryType.LB, ByteOffset = 0x0EC0 },
                ["Device.LastError"] = new() { MemoryType = MemoryType.LW, ByteOffset = 0x01420 },

                // 通信地址
                ["Comm.RequestQueue"] = new() { MemoryType = MemoryType.LB, ByteOffset = 0x03C0 },
                ["Comm.ResponseQueue"] = new() { MemoryType = MemoryType.LB, ByteOffset = 0x0400 }
            };
        }

        public void SendCommand(string command, object parameters)
        {
            var request = new DeviceRequest(
                type: RequestType.ControlCommand,
                source: _stationId,
                payload: new { Command = command, Parameters = parameters }, // 封装到 Payload
                target: null)
            {
                RequestId = _stationId
            };
            _coordinator.QueueDeviceRequest(request);
        }

        public void UpdateSafetyStatus(bool isSafe)
        {
            // 使用标准地址映射中的安全状态地址
            var address = _standardAddressMap["Device.SafetyStatus"];
            WriteToDevice(address, isSafe ? 1 : 0);
        }

        public DeviceAddress GetParameterAddress(string taskType)
        {
            // 根据任务类型返回参数存储地址
            return _standardAddressMap[$"Task.{taskType}.Params"];
        }

        public void WriteParameters(DeviceAddress address, Dictionary<string, object> parameters)
        {
            // 将参数写入设备内存
            foreach (var param in parameters)
            {
                WriteToDevice(address, param.Value);
                address.ByteOffset += GetParameterSize(param.Value); // 移动到下一个参数位置
            }
        }

        public int ReadStatus(string statusKey)
        {
            // 从标准地址读取状态值
            return ReadFromDevice(_standardAddressMap[statusKey]);
        }

        // 私有辅助方法
        private void WriteToDevice(DeviceAddress address, object value)
        {
            // 实际设备写入逻辑（需根据设备协议实现）
        }

        private int ReadFromDevice(DeviceAddress address)
        {
            // 实际设备读取逻辑（需根据设备协议实现）
            return 0; // 示例返回值
        }

        private int GetParameterSize(object value)
        {
            // 根据参数类型返回所需字节大小
            return value switch
            {
                int _ => 4,
                float _ => 4,
                bool _ => 1,
                _ => 2 // 默认大小
            };
        }
    }
}
