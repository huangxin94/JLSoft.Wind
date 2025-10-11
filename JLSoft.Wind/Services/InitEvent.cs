using HWLPAPI;
using JLSoft.Wind.Logs;
using RobotSocketAPI;
using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace JLSoft.Wind.Services
{
    public class InitEvent
    {
        /// <summary>
        /// 注册事件处理
        /// </summary>
        public static void InitializeEvent()
        {
            Leisai_Axis._leisai.OnDeviceError += Leisai_Axis_OnDeviceError;

            HWRobot._robot.OnCommandSent += RobotTCP_OnCommandSent;
            HWRobot._robot.OnResponseReceived += RobotTCP_OnResponseReceived;

            HWAligner._aligner.OnDeviceError += WaferAligner_OnDeviceError;
            HWAligner._aligner.OnConnectionError += WaferAligner_OnConnectionError;
            HWAligner._aligner.OnTimeoutError += WaferAligner_OnTimeoutError;
            HWAligner._aligner.OnCommandSent += WaferAligner_OnCommandSent;
            HWAligner._aligner.OnResponseReceived += WaferAligner_OnResponseReceived;

            //AngleT._angleT.OnDeviceError += WaferAngleT_OnDeviceError;
            //AngleT._angleT.OnConnectionError += WaferAngleT_OnConnectionError;
            //AngleT._angleT.OnCommandSent += WaferAngleT_OnCommandSent;
            //AngleT._angleT.OnResponseReceived += WaferAngleT_OnResponseReceived;

            OCR_Aligner._iv4OCR_Aligner.OnCommandSent += Iv4OCR_Aligner_OnCommandSent;
            OCR_Aligner._iv4OCR_Aligner.OnResponseReceived += Iv4OCR_Aligner_OnResponseReceived;

            OCR_AngleT._iv4OCR_AngleT.OnCommandSent += Iv4OCR_AngleT_OnCommandSent;
            OCR_AngleT._iv4OCR_AngleT.OnResponseReceived += Iv4OCR_AngleT_OnResponseReceived;

            HWLoadPort_1._LP1.ResponseReceived += LP1_Communicator_ResponseReceived;
            HWLoadPort_1._LP1.SendEventArgs += LP1_LoadPort_OnSendEventArgs;
            HWLoadPort_1._LP1.ReadEventArgs += LP1_LoadPort_OnResponseReceived;
            HWLoadPort_1._LP1.StateChanged += LP1_Communicator_StateChanged;
            HWLoadPort_1._LP1.ErrorReceived += LP1_Communicator_ErrorReceived;

            HWLoadPort_2._LP2.ResponseReceived += LP2_Communicator_ResponseReceived;
            HWLoadPort_2._LP2.SendEventArgs += LP2_LoadPort_OnSendEventArgs;
            HWLoadPort_2._LP2.ReadEventArgs += LP2_LoadPort_OnResponseReceived;
            HWLoadPort_2._LP2.StateChanged += LP2_Communicator_StateChanged;
            HWLoadPort_2._LP2.ErrorReceived += LP2_Communicator_ErrorReceived;
        }

        /// <summary>
        /// 当前时间
        /// </summary>
        /// <returns></returns>
        public static string DataNow()
        {
            return DateTime.Now.ToString("HH:mm:ss:ff");
        }

        /// <summary>
        /// CMD信息显示
        /// </summary>
        /// <param name="msg"></param>
        private static void CMD_Info( ListBox listBox, string msg )
        {
            if ( msg != null )
            {
                if ( listBox.InvokeRequired )
                {
                    listBox.Invoke(new Action(() =>
                    {
                        if ( listBox.Items.Count >= 300 )
                        {
                            listBox.Items.Clear();
                        }
                        listBox.Items.Add($"[{DataNow()}] {msg.Trim()}");
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                        listBox.SelectedIndex = -1;
                        if ( msg.Contains("Error") )
                        {
                        }
                    }));
                }
                else
                {
                    if ( listBox.Items.Count >= 300 )
                    {
                        listBox.Items.Clear();
                    }
                    listBox.Items.Add($"[{DataNow()}] {msg.Trim()}");
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                    listBox.SelectedIndex = -1;
                    if ( msg.Contains("Error") )
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI状态
        /// </summary>
        private static void LP1_UpdateUIState()
        {
            bool isConnected = HWLoadPort_1._LP1.State == CommunicationState.Open;
            bool isBusy = HWLoadPort_1._LP1.State == CommunicationState.Sending ||
                         HWLoadPort_1._LP1.State == CommunicationState.WaitingForACK ||
                         HWLoadPort_1._LP1.State == CommunicationState.WaitingForINF;
        }

        private static void LP2_UpdateUIState()
        {
            bool isConnected = HWLoadPort_2._LP2.State == CommunicationState.Open;
            bool isBusy = HWLoadPort_2._LP2.State == CommunicationState.Sending ||
                         HWLoadPort_2._LP2.State == CommunicationState.WaitingForACK ||
                         HWLoadPort_2._LP2.State == CommunicationState.WaitingForINF;
        }

        /// <summary>
        /// 从字符串生成安全的文件名，移除不允许的字符
        /// </summary>
        private static string GetSafeFileName( string input )
        {
            if ( string.IsNullOrEmpty(input) ) return "Unknown";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("", input.Split(invalidChars));
        }

        /// <summary>
        /// 保存系统日志到文件
        /// </summary>
        /// <param name="device">设备类型或日志分类</param>
        /// <param name="message">日志内容</param>
        public static void UI_System_Log_Save( string device, string message )
        {
            // 确保日志目录存在
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Log");
            Directory.CreateDirectory(logDirectory);

            // 构建安全的文件路径
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string safeDeviceName = GetSafeFileName(device);
            string filePath = Path.Combine(logDirectory, $"{date} {safeDeviceName}.txt");

            try
            {
                using ( StreamWriter writer = File.AppendText(filePath) )
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    writer.WriteLine($"[{timestamp}] {message}");
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine($"日志写入失败: {ex.Message}");
                // 可以考虑添加备用日志记录方式
            }
        }

        private static string LastAxisErrorMessage;

        private static void Leisai_Axis_OnDeviceError( object sender, DMCE3000.DeviceErrorEventArgs e )
        {
            if ( LastAxisErrorMessage != e.ErrorMessage.ToString() )
            {
                //Form1.SystemInfo = $"<Axis> [Error] 错误信息: {e.ErrorMessage}";
                LogManager.Log($"<Axis> [Error] 错误信息: {e.ErrorMessage}", LogLevel.Error);
                LastAxisErrorMessage = e.ErrorMessage.ToString();
            }
        }

        /// <summary>
        /// Robot 发送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RobotTCP_OnCommandSent( object sender, RobotSocketClient.LogEventArgs e )
        {
            string info = $" <Robot> [Command] {e.Message}";
            //CMD_Info(Form1._instance.Cmd_Info_listbox, info);
            UI_System_Log_Save("Robot", info);
        }

        /// <summary>
        /// Robot 接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void RobotTCP_OnResponseReceived( object sender, RobotSocketClient.LogEventArgs e )
        {
            try
            {
                string info = $" <Robot> [Response] {e.Message}";
                UI_System_Log_Save("Robot", info);
                if ( e.Message.Contains("?") )
                {
                    var code = await HWRobot.Robot_ReadErrInfoAsync();
                    List<string> errinfo = HWRobot._robot.Robot_Err_switch(code);
                    foreach ( var item in errinfo )
                    {
                        LogManager.Log($"<Robot> [Error] {item}", LogLevel.Error);
                    }
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Aligner 接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WaferAligner_OnResponseReceived( object sender, LogEventArgs e )
        {
            string info = $"[{DataNow()}] <Aligner> [Response] {e.Message}";
            UI_System_Log_Save("Aligner", info);
        }

        /// <summary>
        /// Aligner 发送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WaferAligner_OnCommandSent( object sender, LogEventArgs e )
        {
            string info = $"[{DataNow()}] <Aligner> [Command] {e.Message}";
            UI_System_Log_Save("Aligner", info);
        }

        /// <summary>
        /// Aligner 超时错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WaferAligner_OnTimeoutError( object sender, TimeoutErrorEventArgs e )
        {
            LogManager.Log($"<ALigner> [Error] 连接错误: {e.ErrorMessage}", LogLevel.Error);
        }

        /// <summary>
        /// Aligner 连接错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WaferAligner_OnConnectionError( object sender, ConnectionErrorEventArgs e )
        {
            LogManager.Log($"<ALigner> [Error] 设备错误: {e.ErrorMessage}", LogLevel.Error);
        }

        /// <summary>
        /// Aligner 设备错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WaferAligner_OnDeviceError( object sender, DeviceErrorEventArgs e )
        {
            LogManager.Log($"<ALigner> [Error] 超时错误: {e.ErrorMessage}", LogLevel.Error);
        }

        /// <summary>
        /// Aligner_OCR 发送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Iv4OCR_Aligner_OnCommandSent( object sender, IV4OCRAPI.LogEventArgs e )
        {
            string info = $"[{DataNow()}] <Aligner_OCR> {e.Message}";
            UI_System_Log_Save("Aligner_OCR", info);
        }

        /// <summary>
        /// Aligner_OCR 接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Iv4OCR_AngleT_OnResponseReceived( object sender, IV4OCRAPI.LogEventArgs e )
        {
            string info = $"[{DataNow()}] <Aligner_OCR> {e.Message}";
            UI_System_Log_Save("Aligner_OCR", info);
            try
            {
                string str = e.Message.Replace("\n", "");
                if ( str.Contains("RT") )
                {
                    WaferID_UI.Ocr_str_UI(str);
                    List<string> list = str.Split(',').ToList();
                    if ( list[7] == "OK" )
                    {
                        string oCR_Code_Now = list[9].Replace(" ", "");
                        //OCR_Code_Now = oCR_Code_Now.Replace("\r", "");
                    }
                }
                MainForm._instance.but_OCRStatue.BackColor = Color.Yellow;
                MainForm._instance.but_OCRStatue.Text = "Ready";
            }
            catch
            {
                MainForm._instance.but_OCRStatue.BackColor = Color.Red;
                MainForm._instance.but_OCRStatue.Text = "Error";
            }
        }

        /// <summary>
        /// AngleT_OCR 发送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Iv4OCR_AngleT_OnCommandSent( object sender, IV4OCRAPI.LogEventArgs e )
        {
            string info = $"[{DataNow()}] <AngleT_OCR> {e.Message}";
            UI_System_Log_Save("AngleT_OCR", info);
        }

        /// <summary>
        /// AngleT_OCR 接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Iv4OCR_Aligner_OnResponseReceived( object sender, IV4OCRAPI.LogEventArgs e )
        {
            string info = $"[{DataNow()}] <AngleT_OCR> {e.Message}";
            UI_System_Log_Save("AngleT_OCR", info);

            try
            {
                string str = e.Message.Replace("\n", "");
                if ( str.Contains("RT") )
                {
                    WaferID_UI.Ocr_str_UI(str);
                    List<string> list = str.Split(',').ToList();
                    if ( list[7] == "OK" )
                    {
                        string oCR_Code_Now = list[9].Replace(" ", "");
                        //OCR_Code_Now = oCR_Code_Now.Replace("\r", "");
                    }
                }
                MainForm._instance.but_OCRStatue.BackColor = Color.Yellow;
                MainForm._instance.but_OCRStatue.Text = "Ready";
            }
            catch
            {
                MainForm._instance.but_OCRStatue.BackColor = Color.Red;
                MainForm._instance.but_OCRStatue.Text = "Error";
            }
        }

        /// <summary>
        /// LP1 接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void LP1_Communicator_ResponseReceived( object sender, ResponseReceivedEventArgs e )
        {
            if ( e.Message.Success )
            {
                Console.WriteLine($"收到响应: {e.Message.ResponseType} - {e.Message.RawCommand}");
                string info = $"<LP1>{e.Message.ResponseType}:{e.Message.RawCommand}";
                UI_System_Log_Save("LP1", info);
            }
            else
            {
                Console.WriteLine($"接收错误: {e.Message.ErrorMessage}");
            }
        }

        /// <summary>
        /// LP1 发送事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void LP1_LoadPort_OnSendEventArgs( object sender, SendEventArgs e )
        {
            string info = $" <LoadPort1> [Send] {e.SendStr}";
            UI_System_Log_Save("LP1", info);
        }

        private static void LP1_LoadPort_OnResponseReceived( object sender, ReadEventArgs e )
        {
            string info = $" <LoadPort1> [Response] {e.ReadStr}";
            UI_System_Log_Save("LP1", info);
        }

        private static void LP1_Communicator_StateChanged( object sender, CommunicationStateChangedEventArgs e )
        {
            MainForm._instance.Invoke(new Action(() =>
            {
                Console.WriteLine($"状态: {e.NewState}");
                Console.WriteLine($"通信状态变更: {e.OldState} → {e.NewState}");
                LP1_UpdateUIState();
            }));
        }

        private static void LP1_Communicator_ErrorReceived( object sender, ErrorInfoEventArgs e )
        {
            string info = $"<LoadPort1> {e.ErrorInfoStr}";
            LogManager.Log(info, LogLevel.Error);
        }

        private static void LP2_Communicator_ResponseReceived( object sender, ResponseReceivedEventArgs e )
        {
            if ( e.Message.Success )
            {
                Console.WriteLine($"收到响应: {e.Message.ResponseType} - {e.Message.RawCommand}");
                string info = $"<LP2>{e.Message.ResponseType}:{e.Message.RawCommand}";
                UI_System_Log_Save("LP2", info);
            }
            else
            {
                Console.WriteLine($"接收错误: {e.Message.ErrorMessage}");
            }
        }

        private static void LP2_LoadPort_OnSendEventArgs( object sender, SendEventArgs e )
        {
            string info = $" <LoadPort2> [Send] {e.SendStr}";
            UI_System_Log_Save("LP2", info);
        }

        private static void LP2_LoadPort_OnResponseReceived( object sender, ReadEventArgs e )
        {
            string info = $" <LoadPort2> [Response] {e.ReadStr}";
            UI_System_Log_Save("LP2", info + "\r\n");
        }

        private static void LP2_Communicator_StateChanged( object sender, CommunicationStateChangedEventArgs e )
        {
            MainForm._instance.Invoke(new Action(() =>
            {
                Console.WriteLine($"状态: {e.NewState}");
                Console.WriteLine($"通信状态变更: {e.OldState} → {e.NewState}");
                LP2_UpdateUIState();
            }));
        }

        private static void LP2_Communicator_ErrorReceived( object sender, ErrorInfoEventArgs e )
        {
            string info = $"<LoadPort2> {e.ErrorInfoStr}";
            LogManager.Log(info, LogLevel.Error);
        }
    }
}