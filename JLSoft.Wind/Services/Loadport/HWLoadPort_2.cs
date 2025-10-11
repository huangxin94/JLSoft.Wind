using HWLPAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using JLSoft.Wind.Logs;
using static JLSoft.Wind.Database.StationName;
using JLSoft.Wind.Adapter;

namespace JLSoft.Wind.Services
{
    public class HWLoadPort_2
    {
        public static ILoadPortCommunicator _LP2;

        public static string MappingStateLp2 { get; set; }

        /// <summary>
        /// LoadPortRs232通信开启
        /// </summary>
        public static async Task<bool> LoadPort2_Connect()
        {
            IProtocolParser parser = new LoadPortProtocolParser();
            _LP2 = new LoadPortCommunicator(parser);

            try
            {
                if ( _LP2.State == CommunicationState.Closed || _LP2.State == CommunicationState.Error )
                {
                    // 配置通信参数
                    _LP2.PortName = ConfigService.GetLoadPort2ComPort();
                    _LP2.BaudRate = 9600;

                    // 连接
                    Console.Write("正在连接...");

                    await _LP2.OpenAsync();

                    Console.Write("连接成功");

                    return true;
                }
                else
                {
                    // 断开连接
                    Console.Write("正在断开连接...");

                    await _LP2.CloseAsync();

                    Console.Write("连接已断开");

                    return false;
                }
            }
            catch ( Exception ex )
            {
                Console.Write($"连接错误: {ex.Message}");
                MessageBox.Show($"连接错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> LoadPort2_Disconnect()
        {
            try
            {
                if ( _LP2.State == CommunicationState.Open )
                {
                    // 断开连接
                    Console.Write("正在断开连接...");
                    await _LP2.CloseAsync();
                    Console.Write("连接已断开");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch ( Exception ex )
            {
                Console.Write($"断开连接错误: {ex.Message}");
                MessageBox.Show($"断开连接错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task LoadPort2_ReadStateAsync()
        {
            await _LP2.LPSendStateAsync();
        }

        /// <summary>
        ///  LoadPort1初始化
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> LoadPort2_InitAsync( CancellationToken cts = default )
        {
            int step = 1;
            bool result = false;

            while ( step <= 3 )
            {
                switch ( step )
                {
                    case 1:
                        result = await LoadPort2_ClearingErrorAsync();
                        break;

                    case 2:
                        result = await LoadPort2_HomeAsync();
                        break;

                    case 3:
                        break;
                }
                await Task.Delay(100);
                step++;
                if ( !result )
                {
                    await LoadPort2_ReadStateAsync();
                    result = false;
                }
            }
            //Form1.SystemInfo = result ? "Lp2初始化成功" : "Lp1初始化失败";
            LogManager.Log(result ? "Lp2初始化成功" : "Lp2初始化失败",Sunny.UI.LogLevel.Info);
            return result;
        }

        public static async Task<bool> LoadPort2_ClearingErrorAsync()
        {
            try
            {
                await _LP2.LPSendClearErrorAsync();
                return true;
            }
            catch ( Exception ex )
            {
                Console.Write($"清除错误: {ex.Message}");
                MessageBox.Show($"清除错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> LoadPort2_HomeAsync( CancellationToken cts = default )
        {
            try
            {
                await _LP2.LPSendHomeAsync();
                return true;
            }
            catch ( Exception ex )
            {
                Console.Write($"回原点错误: {ex.Message}");
                MessageBox.Show($"回原点错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> LoadPort2_LoadAsync()
        {
            try
            {
                await _LP2.LPSendLoadAsync();
                return true;
            }
            catch ( Exception ex )
            {
                Console.Write($"装载错误: {ex.Message}");
                MessageBox.Show($"装载错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> LoadPort2_UnloadAsync()
        {
            try
            {
                await _LP2.LPSendUnloadAsync();
                return true;
            }
            catch ( Exception ex )
            {
                Console.Write($"卸载错误: {ex.Message}");
                MessageBox.Show($"卸载错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// LoadPort2读取映射
        /// </summary>
        /// <returns></returns>
        public static async Task LP2ReadMappingAsync()
        {
            try
            {
                // 等待发送映射请求完成
                var flag = _LP2.LPSendMappingAsync();
                await Task.WhenAll(flag);

                // 处理映射字符串
                MappingStateLp2 = _LP2.MappingString;

                // 解析Station枚举并校验
                if ( !System.Enum.TryParse<Station>("LoadPort2", out Station station) )
                {
                    // 处理解析失败（如日志记录）
                    Console.WriteLine("解析Station枚举失败：LoadPort2不存在");
                    return;
                }

                // 更新UI（WinForms环境下确保在UI线程执行）
                EventAggregator.RequestLoadPortColorUpdate("LoadPort2", MappingStateLp2);
            }
            catch ( Exception ex )
            {
                // 处理异常（日志记录等）
                Console.WriteLine($"处理LoadPort2映射时发生错误：{ex.Message}");
                // 根据需要决定是否重新抛出异常
                // throw;
            }
        }

        public static async Task<bool> LoadPort2_MapLoadAsync()
        {
            try
            {
                await _LP2.LPSendMapLoadAsync();
                await Task.Delay(200);
                return true;
            }
            catch ( Exception ex )
            {
                Console.Write($"扫片装载错误: {ex.Message}");
                MessageBox.Show($"扫片装载错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> LoadPort2_UnMaploadAsync()
        {
            try
            {
                await _LP2.LPSendMapUnLoadAsync();
                await Task.Delay(200);
                return true;
            }
            catch ( Exception ex )
            {
                Console.Write($"扫片卸载错误: {ex.Message}");
                MessageBox.Show($"扫片卸载错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}