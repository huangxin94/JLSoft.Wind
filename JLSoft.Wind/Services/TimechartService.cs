using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Adapter;
using JLSoft.Wind.Database.Models.DataSeries;
using JLSoft.Wind.Database.Models.Timeline;
using JLSoft.Wind.Enum;

namespace JLSoft.Wind.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class TimechartService
    {
        private readonly Dictionary<int, DeviceDataSeries> _deviceSeries = new();
        private readonly CommunicationCoordinator _coordinator;// 通信协调器实例 订阅设备状态更新事件

        public TimechartService(CommunicationCoordinator coordinator)
        {
            _coordinator = coordinator;
            _coordinator.TimechartUpdated += HandleTimelineUpdate;
        }
        /// <summary>
        /// 处理时间线更新事件，更新设备数据系列。统一时间线数据转换为数据点
        /// </summary>
        /// <param name="deviceTimelines"></param>
        private void HandleTimelineUpdate(Dictionary<int, DeviceTimeline> deviceTimelines) // 修改参数名为deviceTimelines（复数）
        {
            foreach (var deviceEntry in deviceTimelines)
            {
                int deviceId = deviceEntry.Key;
                // 获取单个设备的时间线数据（DeviceTimeline对象）
                var timelineData = deviceEntry.Value;  // 这里每个timelineData应包含Timestamp属性

                // 修正：使用timelineData而不是deviceTimelines（原参数名timeline）
                GetDataSeries(deviceId, "Status").AddPoint(
                    timelineData.Timestamp, // 使用timelineData的Timestamp
                    (int)timelineData.Status // 假设Status属性存在
                );

                // 添加其他指标
                GetDataSeries(deviceId, "CPU").AddPoint(
                    timelineData.Timestamp,
                    timelineData.CpuUsage
                );

                GetDataSeries(deviceId, "Memory").AddPoint(
                    timelineData.Timestamp,
                    timelineData.MemoryUsage
                );
            }
        }

        public void AddDeviceData(int deviceId, Dictionary<string, object> metrics)
        {
            foreach (var metric in metrics)
            {
                GetDataSeries(deviceId, metric.Key).AddPoint(
                    DateTime.Now,
                    metric.Value
                );
            }
        }
        /// <summary>
        /// 获取或创建设备数据系列。根据设备ID和系列名称返回对应的数据系列实例。公共访问接口
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="seriesName"></param>
        /// <returns></returns>
        public DataSeries GetDataSeries(int deviceId, string seriesName)
        {
            if (!_deviceSeries.ContainsKey(deviceId))
            {
                _deviceSeries[deviceId] = new DeviceDataSeries(deviceId);
            }

            return _deviceSeries[deviceId].GetSeries(seriesName);
        }
    }
    /// <summary>
    /// 设备数据系列类，管理特定设备的所有数据系列。结构化存储时序数据（状态/CPU/内存等）
    /// </summary>
    public class DeviceDataSeries
    {
        private readonly int _deviceId;
        private readonly Dictionary<string, DataSeries> _series = new();

        public DeviceDataSeries(int deviceId)
        {
            _deviceId = deviceId;

            // 预定义关键系列
            CreateSeries("Status", DataType.Integer);
            CreateSeries("CpuUsage", DataType.Float);
            CreateSeries("MemoryUsage", DataType.Float);
            CreateSeries("TaskQueue", DataType.Integer);
        }

        public DataSeries GetSeries(string name)
        {
            if (!_series.TryGetValue(name, out var series))
            {
                series = CreateSeries(name, DetermineDataType(name));
            }

            return series;
        }

        private DataSeries CreateSeries(string name, DataType type)
        {
            var series = new DataSeries($"{_deviceId}_{name}", type);
            _series[name] = series;
            return series;
        }

        private DataType DetermineDataType(string seriesName)
        {
            // 根据系列名称确定数据类型
            switch (seriesName.ToLower())
            {
                case "status":
                case "taskqueue":
                    return DataType.Integer;
                case "cpuusage":
                case "memoryusage":
                case "temperature":
                    return DataType.Float;
                default:
                    return DataType.Float; // 默认返回浮点型
            }
        }
    }
}
