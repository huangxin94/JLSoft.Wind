using System;
using System.Threading;
using System.Threading.Tasks;
using JLSoft.Wind.Class.Models;
using JLSoft.Wind.Database.DB;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Database.Struct;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services.Connect;
using JLSoft.Wind.Services.Status;
using Sunny.UI;

namespace JLSoft.Wind.Services
{
    /// <summary>
    /// ͨ��ʱ������ִ�������Ż��棩
    /// </summary>
    public class SequenceTaskExecutor
    {
        private readonly PlcConnection _stagePlc;
        private readonly int _deviceIndex;
        private readonly DeviceMonitor _deviceMonitor;
        private readonly string _slot;
        private readonly Positions _coord;

        public SequenceTaskExecutor(PlcConnection stagePlc, int deviceIndex, string slot, Positions coord, DeviceMonitor deviceMonitor)
        {
            _stagePlc = stagePlc;
            _deviceIndex = deviceIndex;
            _deviceMonitor = deviceMonitor;
            _slot = slot;
            _coord = coord;
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
                        LogManager.Log($"�豸 {deviceName} ��ǰ״̬Ϊ���޷�ִ����Ƭ����", LogLevel.Warn);
                        return;
                    }
                }
                else
                {
                    LogManager.Log($"�豸 {deviceName} ״̬δ֪���޷�ִ����Ƭ����", LogLevel.Warn);
                    return;
                }
                // 1. �ȴ�Stage Receive Able On
                await WaitStageSignalOnRS("ReceiveAble", cancellationToken);
                LogManager.Log($"1 Stage Receive Able On", LogLevel.Info);

                // 2. ������׼�����ɲ����Զ��嶯����
                await OnRobotReady();
                LogManager.Log($"2 ������׼�����", LogLevel.Info);

                // 3. ������д��Job Data���ɲ����Զ��嶯����
                await OnRobotWriteJobData(deviceName);
                LogManager.Log($"3 ������д��Job Data���", LogLevel.Info);

                // 4. ������Send Able On
                await SetRobotSignalRS("SendAble", true);
                LogManager.Log($"4 ������ Send Able On", LogLevel.Info);

                // 5. ���豸д��Job Data���ɲ����Զ��嶯����
                //await OnStageWriteJobData();
                //LogManager.Log($"5 ���豸д��Job Data���", LogLevel.Info);

                // 6-1. �ȴ�Stage Shutter State On
                await WaitStageSignalOnRS("ShutterState", cancellationToken);
                LogManager.Log($"6-1 Stage Shutter State On", LogLevel.Info);

                // 6-2. �ȴ�Stage Receive Start On
                await WaitStageSignalOnRS("ReceiveStart", cancellationToken);
                LogManager.Log($"6-2 Stage Receive Start On", LogLevel.Info);

                // 7. ������Send Start On
                await SetRobotSignalRS("SendStart", true);
                LogManager.Log($"7 ������ Send Start On", LogLevel.Info);

                // 8. ������ִ����Ƭ�������ɲ����Զ��嶯����
                await OnPutRobotTransfer(deviceName);
                LogManager.Log($"8 ��������Ƭ�������", LogLevel.Info);

                // 9. �����˸�֪Exist Arm�رգ��ɲ����Զ��嶯����
                await OnRobotArmCheck();
                LogManager.Log($"9 �����˸�֪Exist Arm�ر�", LogLevel.Info);

                // 10-1. ������JobTransferSignal On
                await SetRobotSignalRS("JobTransferSignal", true);
                LogManager.Log($"10-1 ������ JobTransferSignal On", LogLevel.Info);

                // 10-2. �����˷�����ҵ���棨�ɲ����Զ��嶯����
                await OnRobotSendJobReport();
                LogManager.Log($"10-2 �����˷�����ҵ�������", LogLevel.Info);

                // 11. �����˷���ԭ�㣨�ɲ����Զ��嶯����
                await OnRobotReady();
                LogManager.Log($"11 �����˷���ԭ�����", LogLevel.Info);

                // 12. ������Send Complete On
                await SetRobotSignalRS("SendComplete", true);
                LogManager.Log($"12 ������ Send Complete On", LogLevel.Info);

                // 13. �ȴ�Stage Receive Complete On
                await WaitStageSignalOnRS("ReceiveComplete", cancellationToken);
                LogManager.Log($"13 Stage Receive Complete On", LogLevel.Info);

                // 14-1. �ȴ�Stage Receive Able Off
                await WaitStageSignalOffRS("ReceiveAble", cancellationToken);
                LogManager.Log($"14-1 Stage Receive Able Off", LogLevel.Info);
                // 14-2. �ȴ�Stage Receive Start Off
                await WaitStageSignalOffRS("ReceiveStart", cancellationToken);
                LogManager.Log($"14-2 Stage Receive Start Off", LogLevel.Info);
                // 14-3. �ȴ�Stage Receive Complete Off
                await WaitStageSignalOffRS("ReceiveComplete", cancellationToken);
                LogManager.Log($"14-3 Stage Receive Complete Off", LogLevel.Info);
                
                // 15. �ȴ�Stage Shutter State Off
                //await WaitStageSignalOffRS("ShutterState", cancellationToken);
                //LogManager.Log($"15 Stage Shutter State Off", LogLevel.Info);

                // 16-1. ������Send Able Off
                await SetRobotSignalRS("SendAble", false);
                LogManager.Log($"16-1 ������ Send Able Off", LogLevel.Info);
                // 16-2. ������Send Start Off
                await SetRobotSignalRS("SendStart", false);
                LogManager.Log($"16-2 ������ Send Start Off", LogLevel.Info);
                // 16-3. ������Send Complete Off
                await SetRobotSignalRS("SendComplete", false);
                LogManager.Log($"16-3 ������ Send Complete Off", LogLevel.Info);
                // 16-4. ������JobTransferSignal Off
                await SetRobotSignalRS("JobTransferSignal", false);
                LogManager.Log($"16-4 ������ JobTransferSignal Off", LogLevel.Info);

                // 17. �����˽�����ҵ���棨�ɲ����Զ��嶯����
                await OnRobotReady();
                LogManager.Log($"17 �����˽�����ҵ�������", LogLevel.Info);
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
                // 0. ��ȡ�豸����״̬
                var deviceName = ConfigService.GetDeviceName(_deviceIndex);

                var deviceState = _deviceMonitor.GetDeviceState(deviceName);
                if (deviceState != null)
                {
                    if (!deviceState.UpstreamInline || !deviceState.UpstreamTrouble)
                    {
                        LogManager.Log($"�豸 {deviceName} ��ǰ״̬���޷�ִ����Ƭ����", LogLevel.Warn);
                        return;
                    }
                }
                else
                {
                    LogManager.Log($"�豸 {deviceName} ״̬δ֪���޷�ִ����Ƭ����", LogLevel.Warn);
                    return;
                }
                // 1. �ȴ�Stage Send Able On
                await WaitStageSignalOnSR("SendAble", cancellationToken);
                LogManager.Log($"2 Stage Send Able On", LogLevel.Info);

                // 2. ������׼�����ɲ����Զ��嶯����
                await OnRobotReady();
                LogManager.Log($"3 ������׼�����", LogLevel.Info);

                // 3. �����˼��Job Data���ɲ����Զ��嶯����
                await OnRobotReadJobData(deviceName);
                LogManager.Log($"4 �����˼��Job Data���", LogLevel.Info);

                // 4. ������Receive Able On
                await SetRobotSignalSR("ReceiveAble", true);
                LogManager.Log($"5 ������ Receive Able On", LogLevel.Info);

                // 6-1. �ȴ�Stage Shutter State On
                await WaitStageSignalOnSR("ShutterState", cancellationToken);
                LogManager.Log($"6-1 Stage Shutter State On", LogLevel.Info);

                // 6-2. �ȴ�Stage Send Start On
                await WaitStageSignalOnSR("SendStart", cancellationToken);
                LogManager.Log($"6-2 Stage Send Start On", LogLevel.Info);

                // 7. ������Receive Start On
                await SetRobotSignalSR("ReceiveStart", true);
                LogManager.Log($"7 ������ Receive Start On", LogLevel.Info);

                // 8. ������ִ����Ƭ�������ɲ����Զ��嶯����
                await OnGetRobotTransfer(deviceName);
                LogManager.Log($"8 ������ȡƬ�������", LogLevel.Info);

                // 9. �����˸�֪Exist Arm�رգ��ɲ����Զ��嶯����
                await OnRobotArmCheck();
                LogManager.Log($"9 �����˸�֪Exist Arm�ر�", LogLevel.Info);

                // 10-1. ������JobTransferSignal On
                await SetRobotSignalSR("JobTransferSignal", true);
                LogManager.Log($"10-1 ������ JobTransferSignal On", LogLevel.Info);

                // 10-2. �����и�����Ҫ�ȴ��Ĳ������豸������ת��
                //await OnRobotSendJobReport();
                //LogManager.Log($"10-2 �����˷�����ҵ�������", LogLevel.Info);

                // 11. ��������ҵ���
                await OnRobotReturnOrigin();
                LogManager.Log($"11 �����˼�¼��������ҵ���", LogLevel.Info);

                // 12. ������Receive Complete On
                await SetRobotSignalSR("ReceiveComplete", true);
                LogManager.Log($"12 ������ Receive Complete On", LogLevel.Info);

                // 13. �ȴ�Stage Receive Complete On
                await WaitStageSignalOnSR("SendComplete", cancellationToken);
                LogManager.Log($"13 Stage Send Complete On", LogLevel.Info);

                // 14-1. �ȴ�Stage Receive Able Off
                await WaitStageSignalOffSR("SendAble", cancellationToken);
                LogManager.Log($"14-1 Stage Send Able Off", LogLevel.Info);
                // 14-2. �ȴ�Stage Receive Start Off
                await WaitStageSignalOffSR("SendStart", cancellationToken);
                LogManager.Log($"14-2 Stage Send Start Off", LogLevel.Info);
                // 14-3. �ȴ�Stage Receive Complete Off
                await WaitStageSignalOffSR("SendComplete", cancellationToken);
                LogManager.Log($"14-3 Stage Send Complete Off", LogLevel.Info);

                // 15. �ȴ�Stage Shutter State Off
                //await WaitStageSignalOffSR("ShutterState", cancellationToken);
                //LogManager.Log($"15 Stage Shutter State Off", LogLevel.Info);

                // 16-1. ������Send Able Off
                await SetRobotSignalSR("ReceiveAble", false);
                LogManager.Log($"16-1 ������ Receive Able Off", LogLevel.Info);
                // 16-2. ������Send Start Off
                await SetRobotSignalSR("ReceiveStart", false);
                LogManager.Log($"16-2 ������ Receive Start Off", LogLevel.Info);
                // 16-3. ������Send Complete Off
                await SetRobotSignalSR("ReceiveComplete", false);
                LogManager.Log($"16-3 ������ Receive Complete Off", LogLevel.Info);
                // 16-4. ������JobTransferSignal Off
                await SetRobotSignalSR("JobTransferSignal", false);
                LogManager.Log($"16-4 ������ JobTransferSignal Off", LogLevel.Info);

                await OnRobotReady();
                LogManager.Log($"16-5 �����˷���ԭ��", LogLevel.Info);
                // 17. �����˽�����ҵ���棨�ɲ����Զ��嶯����
                await OnRobotReceiveJobReport();
                LogManager.Log($"17 �����˽�����ҵ�������", LogLevel.Info);
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

        // �ȴ��ź�On
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

        // �ȴ��ź�Off
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
        // ���û������ź�
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

        // �ȴ��ź�Off
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
        // ���û������ź�
        private async Task SetRobotSignalSR(string signal, bool on)
        {
            string addr = DeviceAddressHelper.GetRobotSignalAddress(signal, (_deviceIndex-1));
            await _stagePlc.Plc.WriteBitAsync(addr, on);

        }

        // ����չ�Ķ����㣨����д��ί�У�
        /// <summary>
        /// ������׼�����ִ�еĶ���
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
                LogManager.Log("ROBOT: ������δ���ӣ��޷�ִ��׼������", LogLevel.Warn, "Robot.Main");
            }
        }
        /// <summary>
        /// ��ȡ������Job Data
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
                    LogManager.Log($"�豸{deviceCode} Job Data��Ч:");
                    LogManager.Log($"JOB ID: {jobData.JOB_ID}");
                    LogManager.Log($"����: {GetTypeDescription(jobData.Type)}");
                    LogManager.Log($"V�Ƴ����: {jobData.ProcessFlag.V_Processed}");
                }
                else
                {
                    LogManager.Log($"�豸{deviceCode} Job Data��Ч!");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"�豸{deviceCode}���ʧ��: {ex.Message}");
            }
        }
        /// <summary>
        /// ������д��Job Data
        /// </summary>
        /// <returns></returns>
        public async Task OnRobotWriteJobData(string deviceCode)
        {
            var config = DeviceConfigManager.GetDeviceConfig(deviceCode);
            if (config == null) return;

            var reader = new JobDataReader(_stagePlc, config);

            try
            {
                var jobData = JobDataRecord.CurrentJobData; // ��ȡ��ǰ��ҵ����
                if (jobData == null)
                {
                    jobData = new JobDataRecord
                    {
                        JOB_ID = "JOB12345", // ʾ������
                        JOBID_Pair = "PAIR12345",
                        VersionNumber = 3,
                        Type = 1, // W����
                        Style = 0,
                        ProcessFlag = new JobProcessFlag(),
                        PPID = 123 // ʾ��PPID
                    };
                }
                

                await reader.WriteJobDataAsync(jobData);
                
            }
            catch (Exception ex)
            {
                LogManager.Log($"�豸{deviceCode}д��ʧ��: {ex.Message}");
            }
        }

        private string GetTypeDescription(int type)
        {
            return type switch
            {
                1 => "W",
                2 => "G",
                3 => "Assembled",
                _ => "δ֪"
            };
        }
        protected virtual Task OnStageWriteJobData() => Task.CompletedTask;
        /// <summary>
        /// �ƶ�λ�û����˵�׼��λ�� ȡƬ
        /// </summary>
        /// <returns></returns>
        private async Task OnGetRobotTransfer(string deviceCode)
        {
            var robotManager = RobotManager.Instance;
            if (robotManager != null && robotManager.IsConnected)
            {
                await RobotManager.Instance.RobotClient.AxisAndRobotGetAsync(deviceCode, _slot, Convert.ToInt32(_coord.X), Convert.ToInt32(_coord.Y), Convert.ToInt32(_coord.Z));
            }
            else
            {
                LogManager.Log("ROBOT: ������δ���ӣ��޷�ִ��׼������", LogLevel.Warn, "Robot.Main");
            }
        }

        /// <summary>
        /// �ƶ�λ�û����˵�׼��λ�� ��Ƭ
        /// </summary>
        /// <returns></returns>
        private async Task OnPutRobotTransfer(string deviceCode)
        {
            var robotManager = RobotManager.Instance;
            if (robotManager != null && robotManager.IsConnected)
            {
                await RobotManager.Instance.RobotClient.AxisAndRobotGetAsync(deviceCode, _slot, Convert.ToInt32(_coord.X), Convert.ToInt32(_coord.Y), Convert.ToInt32(_coord.Z));
            }
            else
            {
                LogManager.Log("ROBOT: ������δ���ӣ��޷�ִ��׼������", LogLevel.Warn, "Robot.Main");
            }
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