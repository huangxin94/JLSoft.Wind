using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JLSoft.Wind;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;

namespace JLSoft.Wind.Services
{
    public class HWAligner
    {
        public static WaferAligner _aligner = new WaferAligner();

        /// <summary>
        /// Aligner开启端口
        /// </summary>
        public static async Task<bool> Aligner_Connect()
        {
            return await _aligner.ConnectAsync(ConfigService.GetAlignerComPort());
        }

        /// <summary>
        /// Aligner关闭端口
        /// </summary>
        public static void Aligner_Disconnect()
        {
            _aligner.Disconnect();
        }

        /// <summary>
        /// Aligner复位
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Aligner_InitAsync( CancellationToken cts = default )
        {
            await _aligner.ClearAlarmAsync();
            await _aligner.HomingAsync();
            await _aligner.MoveToCenterAsync();
            //Form1.SystemInfo = "Aligner初始化成功";
            LogManager.Log("Aligner初始化成功", Sunny.UI.LogLevel.Info);
            return true;
        }

        /// <summary>
        /// Aligner设置
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Aligner_Setting( CancellationToken cancellationToken = default )
        {
            int step = 1;
            bool isSetting = false;
            try
            {
                while ( step <= 6 ) // 确保最多执行5个步骤
                {
                    // 在每个步骤开始前检查取消请求
                    //cancellationToken.ThrowIfCancellationRequested();

                    switch ( step )
                    {
                        case 1: // 设置晶圆尺寸参数
                            int size;
                            if ( MainForm.CurrentWaferSize == "8英寸" )
                            {
                                size = 8;
                            }
                            else
                            {
                                size = 12;
                            }
                            await _aligner.SetWaferSizeAsync(size);
                            break;

                        case 2: // 设置晶圆材质参数
                            await _aligner.SetAlignmentMaterialAsync(Convert.ToInt32(ConfigService.GetAlignerGLM()));
                            break;

                        case 3: // 设置晶圆类型参数
                            await _aligner.SetWaferTypeAsync(Convert.ToInt32(ConfigService.GetAlignerWT()));
                            break;

                        case 4:
                            await _aligner.SetWaferOrientationAsync(Convert.ToInt32(ConfigService.GetAlignerFWO()));
                            break;

                        case 5: // 保存所有参数
                            await _aligner.SaveSettingsAsync();
                            break;

                        case 6: // 完成设置
                            return isSetting = true;
                    }
                    await Task.Delay(100); // 等待0.1秒
                    // 移动到下一步
                    step++;
                }
            }
            catch ( OperationCanceledException )
            {
                return isSetting = false;
            }
            catch
            {
                return isSetting = false;
            }
            return isSetting;
        }

        /// <summary>
        /// Aligner移动到中心位置
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Aligner_MoveToCenter()
        {
            await _aligner.MoveToCenterAsync();
            return true;
        }

        /// <summary>
        /// Aligner开启真空->寻边->关闭真空
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Aligner_BAL()
        {
            await _aligner.OpenVacuumAsync();
            await Task.Delay(100);
            await _aligner.AlignAsync(3);
            await Task.Delay(100);
            await _aligner.CloseVacuumAsync();
            return true;
        }

        /// <summary>
        /// Aligner 关闭真空
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Aligner_CloseVacAsync()
        {
            await _aligner.CloseVacuumAsync();
            return true;
        }
    }
}