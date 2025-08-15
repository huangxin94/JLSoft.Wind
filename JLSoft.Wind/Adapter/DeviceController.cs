using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.RobotControl;
using TaskStatus = JLSoft.Wind.Enum.TaskStatus;

namespace JLSoft.Wind.Adapter
{
    public class DeviceController
    {
        private readonly DeviceCommunicationAPI _commApi;
        private readonly int _stationId;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="coordinator"></param>
        public DeviceController(int stationId, CommunicationCoordinator coordinator)
        {
            _stationId = stationId;
            _commApi = new DeviceCommunicationAPI(stationId, coordinator);
        }

        public void ExecuteTask(DeviceTask task)
        {
            if (task.RequiresRobot)
            {
                RequestRobotInteraction(task);
            }
            else
            {
                ExecuteLocalTask(task);
            }
        }
        /// <summary>
        /// 请求Robot进行任务处理
        /// </summary>
        /// <param name="task"></param>
        private void RequestRobotInteraction(DeviceTask task)
        {
            // 1. 创建Robot任务
            var robotTask = new RobotTask
            {
                DeviceId = _stationId,
                TaskType = task.Type,
                Parameters = task.Parameters,
                Priority = task.Priority,
                RequiredTools = task.RequiredTools
            };

            // 2. 向Robot发送请求
            _commApi.RequestRobotTask(robotTask);

            // 3. 等待响应或超时
            var response = WaitForResponse(TimeSpan.FromSeconds(10));

            // 4. 根据响应处理
            if (response.Accepted)
            {
                PrepareForRobot(task);
                WaitForRobotCompletion();
                FinalizeTask(task);
            }
            else
            {
                HandleTaskRejection(task);
            }
        }

        private void FinalizeTask(DeviceTask task)
        {
            // 1. 恢复设备安全状态
            SetSafetyStatus(false);

            // 2. 回收工作区域
            _commApi.SendCommand("ReclaimWorkArea", new { DeviceId = _stationId });

            // 3. 更新任务状态
            task.Status = TaskStatus.Completed;
            // 这里添加实际的状态持久化逻辑
        }

        private void HandleTaskRejection(DeviceTask task)
        {
            // 1. 记录任务拒绝日志
            // 以下属性在原始DeviceTask中不存在：
            Console.WriteLine($"Task {task.TaskId} rejected: {task.RejectionReason}"); // 需要TaskId和RejectionReason
            if (task.FallbackAction != null) // 需要FallbackAction
            {
                ExecuteFallbackAction(task.FallbackAction);
            }
            task.Status = TaskStatus.Rejected; // 需要Status
        }

        private void ExecuteFallbackAction(Action fallback)
        {
            try
            {
                fallback?.Invoke();
            }
            catch (Exception ex)
            {
                // 处理回退失败
            }
        }
        private RobotResponse WaitForResponse(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while (!_commApi.HasResponse)
            {
                if (DateTime.Now - start > timeout)
                {
                    return new RobotResponse { Accepted = false, Message = "Timeout waiting for response" };
                }
                Thread.Sleep(100);
            }
            return _commApi.GetResponse();
        }
        private void ExecuteLocalTask(DeviceTask task)
        {
            // 本地任务处理逻辑
            try
            {
                // 执行任务...

                // 完成后通知系统
                _commApi.SignalCompletion();
            }
            catch (Exception ex)
            {
                // 错误处理...
            }
        }
        private void ReleaseWorkArea()
        {
            // 释放工作区域（通过通信API发送命令）
            _commApi.SendCommand("ReleaseWorkArea", new { DeviceId = _stationId });
        }

        private void SetSafetyStatus(bool isSafe)
        {
            // 设置设备安全状态（通过通信API更新安全标志）
            _commApi.UpdateSafetyStatus(isSafe);
        }

        private void ConfigureTaskParameters(DeviceTask task)
        {
            // 配置任务参数（写入设备内存）
            var paramAddress = _commApi.GetParameterAddress(task.Type);
            _commApi.WriteParameters(paramAddress, task.Parameters);
        }

        private bool CheckRobotCompletion()
        {
            // 检查机器人完成状态（通过标准地址映射查询）
            return _commApi.ReadStatus("Robot.CompletionStatus") == 1;
        }
        private void PrepareForRobot(DeviceTask task)
        {
            // 准备接受Robot服务
            // 1. 释放工作区域
            ReleaseWorkArea();

            // 2. 设置安全状态
            SetSafetyStatus(true);

            // 3. 准备任务参数
            ConfigureTaskParameters(task);
        }

        private void WaitForRobotCompletion()
        {
            // 监听Robot完成信号
            const int timeoutMs = 30000;  // 30秒超时
            DateTime startTime = DateTime.Now;

            while (!CheckRobotCompletion())
            {
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
                {
                    throw new TimeoutException("Robot operation timed out");
                }
                Thread.Sleep(100);
            }
        }
    }
}
