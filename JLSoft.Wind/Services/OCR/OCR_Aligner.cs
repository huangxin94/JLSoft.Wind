using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Logs;

namespace JLSoft.Wind.Services
{
    public class OCR_Aligner
    {
        public static IV4SensorClient _iv4OCR_Aligner = new IV4SensorClient();

        /// <summary>
        /// OCR Aligner 开启通信
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OCR_Aligner_Connect()
        {
            var result = await _iv4OCR_Aligner.ConnectAsync(ConfigService.GetAlignerOCRIp());
            return result.success;
        }

        /// <summary>
        /// OCR Aligner 关闭通信
        /// </summary>
        public static void OCR_Aligner_Disconnect()
        {
            _iv4OCR_Aligner.Disconnect();
        }

        /// <summary>
        /// OCR Aligner 初始化
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OCR_Aligner_InitAsync()
        {
            await _iv4OCR_Aligner.ClearErrorAsync();
            var result = await _iv4OCR_Aligner.SetOFAsync();
            if ( result.success )
            {
                //Form1._instance.OCR1_State.BackColor = Color.Yellow;
                //Form1._instance.OCR1_State.Text = "Ready";
                //Form1.SystemInfo = "OCR_Aligner 初始化成功";
                LogManager.Log("OCR_Aligner 初始化成功", Sunny.UI.LogLevel.Info, "OCR_Aligner");
                return true;
            }
            else
            {
                //Form1._instance.OCR1_State.BackColor = Color.Red;
                //Form1._instance.OCR1_State.Text = "Error";
                //Form1.SystemInfo = "OCR_Aligner 初始化失败";
                LogManager.Log($"OCR_Aligner 初始化失败: {result.errorMessage}", Sunny.UI.LogLevel.Warn, "OCR_Aligner");
                return false;
            }
        }

        public static async Task<string> OCR_Aligner_Trig()
        {
            var result = await _iv4OCR_Aligner.TriggerAndReadResultAsync();
            if (result.success)
            {
                var str = result.result.Split(',');
                 string waferID = str[9].ToString().Trim();
                return waferID;
            }
            else
            {
                LogManager.Log($"OCR_Aligner 读码失败: {result.errorMessage}", Sunny.UI.LogLevel.Warn, "OCR_Aligner");
                return string.Empty;
            }
        }
    }
}