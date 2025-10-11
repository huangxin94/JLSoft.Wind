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
    public class OCR_AngleT
    {
        public static IV4SensorClient _iv4OCR_AngleT = new IV4SensorClient();

        /// <summary>
        /// OCR Aligner 开启通信
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OCR_AngleT_Connect()
        {
            var result = await _iv4OCR_AngleT.ConnectAsync(ConfigService.GetAngleTOCRIp());
            return result.success;
        }

        /// <summary>
        /// OCR Aligner 关闭通信
        /// </summary>
        public static void OCR_AngleT_Disconnect()
        {
            _iv4OCR_AngleT.Disconnect();
        }

        /// <summary>
        /// OCR AngleT初始化
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OCR_AngleT_InitAsync()
        {
            await _iv4OCR_AngleT.ClearErrorAsync();
            var result = await _iv4OCR_AngleT.SetOFAsync();
            if ( result.success )
            {
                //Form1._instance.OCR2_State.BackColor = Color.Yellow;
                //Form1._instance.OCR2_State.Text = "Ready";
                LogManager.Log("OCR_AngleT 初始化成功", Sunny.UI.LogLevel.Info, "OCR_AngleT");
                return true;
            }
            else
            {
                //Form1._instance.OCR2_State.BackColor = Color.Red;
                //Form1._instance.OCR2_State.Text = "Error";
                LogManager.Log("OCR_AngleT 初始化失败", Sunny.UI.LogLevel.Warn, "OCR_AngleT");
                return false;
            }
        }

        public static async Task<string> OCR_AngleT_Trig()
        {
            var result = await _iv4OCR_AngleT.TriggerAndReadResultAsync();
            if ( result.success )
            {
                var str = result.result.Split(',');
                string waferID = str[9].ToString().Trim();
                return waferID;
            }
            else
            {
                LogManager.Log("OCR_AngleT 读码失败",Sunny.UI.LogLevel.Warn, "OCR_AngleT");
                return string.Empty;
            }
        }
    }
}