using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DuckDB.NET.Data;
using JLSoft.Wind.Class.Models;
using JLSoft.Wind.Database.DB;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Database.Struct;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services.Connect;
using JLSoft.Wind.Services.DuckDb;
using JLSoft.Wind.Services.Status;
using Sunny.UI;
using static JLSoft.Wind.Database.StationName;

namespace JLSoft.Wind.Services
{
    /// <summary>
    /// 通用时序任务执行器（优化版）
    /// </summary>
    public class SequenceTaskExecutor
    {
        private readonly PlcConnection _stagePlc;
        private readonly int _deviceIndex;
        private readonly DeviceMonitor _deviceMonitor;
        private readonly string _slot;
        private readonly Positions _coord;
        private readonly ProductionTrackingService _trackingService;
        private Guid? _currentMainId = null;
        private readonly DuckDbService _dbService;


        public SequenceTaskExecutor(PlcConnection stagePlc, int deviceIndex, string slot, Positions coord, DeviceMonitor deviceMonitor, DuckDbService dbService)
        {
            _stagePlc = stagePlc;
            _deviceIndex = deviceIndex;
            _deviceMonitor = deviceMonitor;
            _slot = slot;
            _coord = coord;
            _dbService = dbService;
        }

        public async Task PutRunAsync(CancellationToken cancellationToken = default)
        {
            try
            {

                var deviceName = ConfigService.GetDeviceName(_deviceIndex);

                var deviceState = _deviceMonitor.GetDeviceState(deviceName);
                if (deviceState != null)
                {
                    if (!deviceState.DownstreamInline || !deviceState.DownstreamTrouble)
                    {
                        LogManager.Log($"设备 {deviceName} 当前状态为，无法执行送片任务。", LogLevel.Warn);
                        return;
                    }
                }
                else
                {
                    LogManager.Log($"设备 {deviceName} 状态未知，无法执行送片任务。", LogLevel.Warn);
                    return;
                }
                // 1. 等待Stage Receive Able On
                await WaitStageSignalOnRS("ReceiveAble", cancellationToken);
                LogManager.Log($"1 Stage Receive Able On", LogLevel.Info);

                // 2. 机器人准备（可插入自定义动作）
                await OnRobotReady();
                LogManager.Log($"2 机器人准备完成", LogLevel.Info);

                // 3. 机器人写入Job Data（可插入自定义动作）
                await OnRobotWriteJobData(deviceName);
                LogManager.Log($"3 机器人写入Job Data完成", LogLevel.Info);

                // 4. 机器人Send Able On
                await SetRobotSignalRS("SendAble", true);
                LogManager.Log($"4 机器人 Send Able On", LogLevel.Info);

                // 5. 子设备写入Job Data（可插入自定义动作）
                //await OnStageWriteJobData();
                //LogManager.Log($"5 子设备写入Job Data完成", LogLevel.Info);

                // 6-1. 等待Stage Shutter State On
                await WaitStageSignalOnRS("ShutterState", cancellationToken);
                LogManager.Log($"6-1 Stage Shutter State On", LogLevel.Info);

                // 6-2. 等待Stage Receive Start On
                await WaitStageSignalOnRS("ReceiveStart", cancellationToken);
                LogManager.Log($"6-2 Stage Receive Start On", LogLevel.Info);

                // 7. 机器人Send Start On
                await SetRobotSignalRS("SendStart", true);
                LogManager.Log($"7 机器人 Send Start On", LogLevel.Info);

                // 8. 机器人执行送片动作（可插入自定义动作）
                await OnPutRobotTransfer(deviceName);
                LogManager.Log($"8 机器人送片动作完成", LogLevel.Info);

                // 9. 机器人感知Exist Arm关闭（可插入自定义动作）
                await OnRobotArmCheck();
                LogManager.Log($"9 机器人感知Exist Arm关闭", LogLevel.Info);

                // 10-1. 机器人JobTransferSignal On
                await SetRobotSignalRS("JobTransferSignal", true);
                LogManager.Log($"10-1 机器人 JobTransferSignal On", LogLevel.Info);

                // 10-2. 机器人发送作业报告（可插入自定义动作）
                await OnRobotSendJobReport();
                LogManager.Log($"10-2 机器人发送作业报告完成", LogLevel.Info);

                // 11. 机器人返回原点（可插入自定义动作）
                await OnRobotReady();
                LogManager.Log($"11 机器人返回原点完成", LogLevel.Info);

                // 12. 机器人Send Complete On
                await SetRobotSignalRS("SendComplete", true);
                LogManager.Log($"12 机器人 Send Complete On", LogLevel.Info);

                // 13. 等待Stage Receive Complete On
                await WaitStageSignalOnRS("ReceiveComplete", cancellationToken);
                LogManager.Log($"13 Stage Receive Complete On", LogLevel.Info);

                // 14-1. 等待Stage Receive Able Off
                await WaitStageSignalOffRS("ReceiveAble", cancellationToken);
                LogManager.Log($"14-1 Stage Receive Able Off", LogLevel.Info);
                // 14-2. 等待Stage Receive Start Off
                await WaitStageSignalOffRS("ReceiveStart", cancellationToken);
                LogManager.Log($"14-2 Stage Receive Start Off", LogLevel.Info);
                // 14-3. 等待Stage Receive Complete Off
                await WaitStageSignalOffRS("ReceiveComplete", cancellationToken);
                LogManager.Log($"14-3 Stage Receive Complete Off", LogLevel.Info);
                
                // 15. 等待Stage Shutter State Off
                //await WaitStageSignalOffRS("ShutterState", cancellationToken);
                //LogManager.Log($"15 Stage Shutter State Off", LogLevel.Info);

                // 16-1. 机器人Send Able Off
                await SetRobotSignalRS("SendAble", false);
                LogManager.Log($"16-1 机器人 Send Able Off", LogLevel.Info);
                // 16-2. 机器人Send Start Off
                await SetRobotSignalRS("SendStart", false);
                LogManager.Log($"16-2 机器人 Send Start Off", LogLevel.Info);
                // 16-3. 机器人Send Complete Off
                await SetRobotSignalRS("SendComplete", false);
                LogManager.Log($"16-3 机器人 Send Complete Off", LogLevel.Info);
                // 16-4. 机器人JobTransferSignal Off
                await SetRobotSignalRS("JobTransferSignal", false);
                LogManager.Log($"16-4 机器人 JobTransferSignal Off", LogLevel.Info);

                // 17. 机器人接收作业报告（可插入自定义动作）
                await OnRobotReady();
                LogManager.Log($"17 机器人接收作业报告完成", LogLevel.Info);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task GetRunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // 0. 获取设备基础状态
                var deviceName = ConfigService.GetDeviceName(_deviceIndex);

                var deviceState = _deviceMonitor.GetDeviceState(deviceName);
                if (deviceState != null)
                {
                    if (!deviceState.UpstreamInline || !deviceState.UpstreamTrouble)
                    {
                        LogManager.Log($"设备 {deviceName} 当前状态，无法执行送片任务。", LogLevel.Warn);
                        return;
                    }
                }
                else
                {
                    LogManager.Log($"设备 {deviceName} 状态未知，无法执行送片任务。", LogLevel.Warn);
                    return;
                }
                // 1. 等待Stage Send Able On
                await WaitStageSignalOnSR("SendAble", cancellationToken);
                LogManager.Log($"2 Stage Send Able On", LogLevel.Info);

                // 2. 机器人准备（可插入自定义动作）
                await OnRobotReady();
                LogManager.Log($"3 机器人准备完成", LogLevel.Info);

                // 3. 机器人检查Job Data（可插入自定义动作）
                await OnRobotReadJobData(deviceName);
                LogManager.Log($"4 机器人检查Job Data完成", LogLevel.Info);

                // 4. 机器人Receive Able On
                await SetRobotSignalSR("ReceiveAble", true);
                LogManager.Log($"5 机器人 Receive Able On", LogLevel.Info);

                // 6-1. 等待Stage Shutter State On
                await WaitStageSignalOnSR("ShutterState", cancellationToken);
                LogManager.Log($"6-1 Stage Shutter State On", LogLevel.Info);

                // 6-2. 等待Stage Send Start On
                await WaitStageSignalOnSR("SendStart", cancellationToken);
                LogManager.Log($"6-2 Stage Send Start On", LogLevel.Info);

                // 7. 机器人Receive Start On
                await SetRobotSignalSR("ReceiveStart", true);
                LogManager.Log($"7 机器人 Receive Start On", LogLevel.Info);

                // 8. 机器人执行送片动作（可插入自定义动作）
                await OnGetRobotTransfer(deviceName);
                LogManager.Log($"8 机器人取片动作完成", LogLevel.Info);

                // 9. 机器人感知Exist Arm关闭（可插入自定义动作）
                await OnRobotArmCheck();
                LogManager.Log($"9 机器人感知Exist Arm关闭", LogLevel.Info);

                // 10-1. 机器人JobTransferSignal On
                await SetRobotSignalSR("JobTransferSignal", true);
                LogManager.Log($"10-1 机器人 JobTransferSignal On", LogLevel.Info);

                // 10-2. 这里有个不需要等待的步骤是设备自行运转的
                //await OnRobotSendJobReport();
                //LogManager.Log($"10-2 机器人发送作业报告完成", LogLevel.Info);

                // 11. 机器人作业完成
                await OnRobotReturnOrigin();
                LogManager.Log($"11 机器人记录及报告作业完成", LogLevel.Info);

                // 12. 机器人Receive Complete On
                await SetRobotSignalSR("ReceiveComplete", true);
                LogManager.Log($"12 机器人 Receive Complete On", LogLevel.Info);

                // 13. 等待Stage Receive Complete On
                await WaitStageSignalOnSR("SendComplete", cancellationToken);
                LogManager.Log($"13 Stage Send Complete On", LogLevel.Info);

                // 14-1. 等待Stage Receive Able Off
                await WaitStageSignalOffSR("SendAble", cancellationToken);
                LogManager.Log($"14-1 Stage Send Able Off", LogLevel.Info);
                // 14-2. 等待Stage Receive Start Off
                await WaitStageSignalOffSR("SendStart", cancellationToken);
                LogManager.Log($"14-2 Stage Send Start Off", LogLevel.Info);
                // 14-3. 等待Stage Receive Complete Off
                await WaitStageSignalOffSR("SendComplete", cancellationToken);
                LogManager.Log($"14-3 Stage Send Complete Off", LogLevel.Info);

                // 15. 等待Stage Shutter State Off
                //await WaitStageSignalOffSR("ShutterState", cancellationToken);
                //LogManager.Log($"15 Stage Shutter State Off", LogLevel.Info);

                // 16-1. 机器人Send Able Off
                await SetRobotSignalSR("ReceiveAble", false);
                LogManager.Log($"16-1 机器人 Receive Able Off", LogLevel.Info);
                // 16-2. 机器人Send Start Off
                await SetRobotSignalSR("ReceiveStart", false);
                LogManager.Log($"16-2 机器人 Receive Start Off", LogLevel.Info);
                // 16-3. 机器人Send Complete Off
                await SetRobotSignalSR("ReceiveComplete", false);
                LogManager.Log($"16-3 机器人 Receive Complete Off", LogLevel.Info);
                // 16-4. 机器人JobTransferSignal Off
                await SetRobotSignalSR("JobTransferSignal", false);
                LogManager.Log($"16-4 机器人 JobTransferSignal Off", LogLevel.Info);

                await OnRobotReady();
                LogManager.Log($"16-5 机器人返回原点", LogLevel.Info);
                // 17. 机器人接收作业报告（可插入自定义动作）
                await OnRobotReceiveJobReport();
                LogManager.Log($"17 机器人接收作业报告完成", LogLevel.Info);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // 等待信号On
        private async Task WaitStageSignalOnRS(string signal, CancellationToken token)
        {
            string addr = DeviceAddressHelper.PutU1SignalAddress(signal, _deviceIndex);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var result = await _stagePlc.ReadDataAsync(addr, 1);
                if (result.IsSuccess && (result.Content[0] & 0x1) == 1)
                    break;
                await Task.Delay(50, token);
            }

        }

        // 等待信号Off
        private async Task WaitStageSignalOffRS(string signal, CancellationToken token)
        {
            string addr = DeviceAddressHelper.PutU1SignalAddress(signal, _deviceIndex);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var result = await _stagePlc.ReadDataAsync(addr, 1);
                if (result.IsSuccess && (result.Content[0] & 0x1) == 0)
                    break;
                await Task.Delay(50, token);
            }
        }
        // 设置机器人信号
        private async Task SetRobotSignalRS(string signal, bool on)
        {
            string addr = DeviceAddressHelper.PutRobotSignalAddress(signal, _deviceIndex);
            await _stagePlc.Plc.WriteBitAsync(addr, on);

        }


        private async Task WaitStageSignalOnSR(string signal, CancellationToken token)
        {
            string addr = DeviceAddressHelper.GetU1SignalAddress(signal, _deviceIndex);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var result = await _stagePlc.ReadDataAsync(addr, 1);
                if (result.IsSuccess && (result.Content[0] & 0x1) == 1)
                    break;
                await Task.Delay(50, token);
            }

        }

        // 等待信号Off
        private async Task WaitStageSignalOffSR(string signal, CancellationToken token)
        {
            string addr = DeviceAddressHelper.GetU1SignalAddress(signal, _deviceIndex);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var result = await _stagePlc.ReadDataAsync(addr, 1);
                if (result.IsSuccess && (result.Content[0] & 0x1) == 0)
                    break;
                await Task.Delay(50, token);
            }
        }
        // 设置机器人信号
        private async Task SetRobotSignalSR(string signal, bool on)
        {
            string addr = DeviceAddressHelper.GetRobotSignalAddress(signal, (_deviceIndex-1));
            await _stagePlc.Plc.WriteBitAsync(addr, on);

        }

        // 可扩展的动作点（可重写或委托）
        /// <summary>
        /// 机器人准备完成执行的动作
        /// </summary>
        /// <returns></returns>
        private async Task OnRobotReady()
        {

            var robotManager = RobotManager.Instance;
            if (robotManager != null && robotManager.IsConnected)
            {
                await RobotManager.Instance.RobotClient.HOMAsync();
            }
            else
            {
                LogManager.Log("ROBOT: 机器人未连接，无法执行准备动作", LogLevel.Warn, "Robot.Main");
            }
        }
        /// <summary>
        /// 读取机器人Job Data
        /// </summary>
        /// <param name="deviceCode"></param>
        /// <returns></returns>
        public async Task OnRobotReadJobData(string deviceCode)
        {
            var config = DeviceConfigManager.GetDeviceConfig(deviceCode);
            if (config == null) return;

            var reader = new JobDataReader(_stagePlc, config);

            try
            {
                var jobData = await reader.ReadJobDataAsync();

                if (jobData.IsValid())
                {
                    LogManager.Log($"设备{deviceCode} Job Data有效:");
                    LogManager.Log($"JOB ID: {jobData.JOB_ID}");
                    LogManager.Log($"类型: {GetTypeDescription(jobData.Type)}");
                    LogManager.Log($"V制程完成: {jobData.ProcessFlag.V_Processed}");


                    // 2. 检查是否存在主流程数据
                    if (!_currentMainId.HasValue)
                    {
                        // 获取该产品的预设工艺流程
                        var processFlow = GetProcessFlow(jobData.JOB_ID);

                        // 创建主流程和步骤数据
                        _currentMainId = _trackingService.StartProduction(
                            jobData.JOB_ID,
                            jobData.JOBID_Pair,
                            processFlow
                        );
                    }

                    // 3. 记录步骤开始
                    string stepName = $"{deviceCode}_Pick";
                    _trackingService.StartProcessStep(_currentMainId.Value, stepName);
                }
                else
                {
                    LogManager.Log($"设备{deviceCode} Job Data无效!");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"设备{deviceCode}检查失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 机器人写入Job Data
        /// </summary>
        /// <returns></returns>
        public async Task OnRobotWriteJobData(string deviceCode)
        {
            var config = DeviceConfigManager.GetDeviceConfig(deviceCode);
            if (config == null) return;

            var reader = new JobDataReader(_stagePlc, config);

            try
            {
                var jobData = JobDataRecord.CurrentJobData; // 获取当前作业数据
                if (jobData == null)
                {
                    jobData = new JobDataRecord
                    {
                        JOB_ID = "JOB12345", // 示例数据
                        JOBID_Pair = "PAIR12345",
                        VersionNumber = 3,
                        Type = 1, // W类型
                        Style = 0,
                        ProcessFlag = new JobProcessFlag(),
                        PPID = 123 // 示例PPID
                    };
                }
                

                await reader.WriteJobDataAsync(jobData);
                
            }
            catch (Exception ex)
            {
                LogManager.Log($"设备{deviceCode}写入失败: {ex.Message}");
            }
        }

        private string GetTypeDescription(int type)
        {
            return type switch
            {
                1 => "W",
                2 => "G",
                3 => "Assembled",
                _ => "未知"
            };
        }
        protected virtual Task OnStageWriteJobData() => Task.CompletedTask;
        /// <summary>
        /// 移动位置机器人到准备位置 取片
        /// </summary>
        /// <returns></returns>
        private async Task OnGetRobotTransfer(string deviceCode)
        {
            var robotManager = RobotManager.Instance;
            if (robotManager != null && robotManager.IsConnected)
            {
                CancellationToken cts = new CancellationToken();
                System.Enum.TryParse<Station>(deviceCode, out Station station);
                await robotManager.AxisMovStation(deviceCode,_coord);
                await Task.Delay(500);
                await robotManager.Robot_GetAsync(station, deviceCode, cts);
            }
            else
            {
                LogManager.Log("ROBOT: 机器人未连接，无法执行准备动作", LogLevel.Warn, "Robot.Main");
            }
        }

        /// <summary>
        /// 移动位置机器人到准备位置 放片
        /// </summary>
        /// <returns></returns>
        private async Task OnPutRobotTransfer(string deviceCode)
        {
            var robotManager = RobotManager.Instance;
            if (robotManager != null && robotManager.IsConnected)
            {
                CancellationToken cts = new CancellationToken();
                System.Enum.TryParse<Station>(deviceCode, out Station station);
                await robotManager.AxisMovStation(deviceCode, _coord);
                await Task.Delay(500);
                await robotManager.Robot_PutAsync(station, deviceCode, cts);

                if (_currentMainId.HasValue)
                {
                    // 获取当前步骤名称（如设备A1的"Load"操作）
                    string stepName = $"{deviceCode}_Place";
                    string nextStepName = GetNextStepName(deviceCode);
                    // 记录步骤完成时间（触发QTime计算起点）
                    _trackingService.CompleteProcessStep(_currentMainId.Value, stepName);

                    // 记录移动到下一步（触发QTime计算）
                    _trackingService.MoveToNextStep(_currentMainId.Value, nextStepName);
                }
            }
            else
            {
                LogManager.Log("ROBOT: 机器人未连接，无法执行准备动作", LogLevel.Warn, "Robot.Main");
            }

        }
        private string GetNextStepName(string deviceCode)
        {
            // 实际逻辑应根据流程配置确定
            return deviceCode switch
            {
                "A1" => "A2_Process",
                "A2" => "C1_Clean",
                "C1" => "Final_Inspection",
                _ => "Complete"
            };
        }
        private List<ProcessFlowItem> GetProcessFlow(string productType)
        {
            // 从数据库查询（文档11 ProcessFlowFrm）
            string sql = @"SELECT WorkstageName, Seq, Qtime 
                   FROM process_flow_item 
                   WHERE WorkstageName = ?";

            var dt = _dbService.ExecuteQuery(sql, new DuckDBParameter(productType));

            return dt.AsEnumerable().Select(row => new ProcessFlowItem
            {
                WorkstageName = row["WorkstageName"].ToString(),
                Seq = Convert.ToInt32(row["Seq"]),
                Qtime = Convert.ToInt32(row["Qtime"])
            }).ToList();
        }
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnRobotArmCheck() => Task.CompletedTask;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnRobotSendJobReport() => Task.CompletedTask;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnRobotReturnOrigin() => Task.CompletedTask;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnRobotReceiveJobReport() => Task.CompletedTask;
    }
}