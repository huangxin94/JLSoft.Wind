using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleTAPI;
using JLSoft.Wind;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;

namespace JLSoft.Wind.Services
{
    public class AngleT
    {
        public static IAngleTController _angleT;

        /// <summary>
        /// AngleT连接
        /// </summary>
        public static async Task<bool> Connect()
        {
            var ip = ConfigService.GetAngleTIp();
            var port = Convert.ToInt16(ConfigService.GetAngleTPort());
            _angleT = new AngleTController(ip, port);
            return await _angleT.ConnectAsync();
        }

        /// <summary>
        /// AngleT断开连接
        /// </summary>
        public static void Disconnect()
        {
            _angleT.Disconnect();
        }

        /// <summary>
        /// AngleT初始化
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> AngleTInitAsync( CancellationToken cts = default )
        {
            await _angleT.RSTAsync();
            await _angleT.SITAsync(MainForm.CurrentWaferSize == "8英寸" ? "8" : "12", cts);
            await _angleT.ANGAsync(ConfigService.GetAngleTAngle(), cts);
            await _angleT.HOMAsync(cts);
            //Form1.SystemInfo = "AngleT初始化成功";
            LogManager.Log("AngleT初始化成功",Sunny.UI.LogLevel.Info);
            return true;
        }

        /// <summary>
        /// AngleT一键寻边
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> AngleTOneKeyBALAsync( CancellationToken cts = default )
        {
            await _angleT.CVNAsync(cts);
            await Task.Delay(50);
            await _angleT.BALAsync(cts);
            await Task.Delay(50);
            await _angleT.CVFAsync(cts);
            return true;
        }

        /// <summary>
        /// AngleT回原点
        /// </summary>
        /// <returns></returns>
        public static async Task<string> HomeAsync()
        {
            return await _angleT.HOMAsync();
        }

        /// <summary>
        /// AngleT移动至中心位置
        /// </summary>
        /// <returns></returns>
        public static async Task<string> MTMAsync()
        {
            return await _angleT.MTMAsync();
        }

        /// <summary>
        /// AngleT开始寻边
        /// </summary>
        /// <returns></returns>
        public static async Task<string> BalAsync()
        {
            return await _angleT.BALAsync();
        }

        /// <summary>
        /// AngleT关闭真空
        /// </summary>
        /// <returns></returns>
        public static async Task<string> CVFAsync()
        {
            return await _angleT.CVFAsync();
        }

        /// <summary>
        /// AngletT打开真空
        /// </summary>
        /// <returns></returns>
        public static async Task<string> CVNAsync()
        {
            return await _angleT.CVNAsync();
        }
    }
}