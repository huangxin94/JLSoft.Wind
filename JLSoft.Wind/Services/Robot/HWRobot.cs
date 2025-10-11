using JLSoft.Wind.Database;
using JLSoft.Wind.Logs;
using RobotSocketAPI;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JLSoft.Wind.Database.StationName;

namespace JLSoft.Wind.Services
{
    public class HWRobot
    {
        public static RobotSocketClient _robot = new RobotSocketClient(ConfigService.GetRobotIp(), ConfigService.GetRobotPort());

        /// <summary>
        /// 连接机器人
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> TcpConnect_RobotAsync()
        {
            return await _robot.ConnectAsync();
            
        }

        /// <summary>
        /// 断开机器人
        /// </summary>
        public static void TcpClose_Robot()
        {
            _robot?.Disconnect();
        }

        /// <summary>
        /// 机器人初始化
        /// </summary>
        /// <param name="externalCts"></param>
        /// <returns></returns>
        public static async Task<bool> Robot_InitAsync( CancellationToken cts )
        {
            await _robot.REMSAsync();
            await _robot.SVONAsync();
            await _robot.HOMAsync();
            //Form1.SystemInfo = "Robot初始化成功";
            LogManager.Log("Robot初始化成功", Sunny.UI.LogLevel.Info, "Robot.Main");
            return true;
        }

        /// <summary>
        /// 机器人清除报警
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_RemsAsync( CancellationToken cts = default(CancellationToken) )
        {
            await _robot.REMSAsync(cts);
        }

        /// <summary>
        /// 机器人读取错误信息
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_ReadErrInfoAsync( CancellationToken cts = default(CancellationToken) )
        {
            return await _robot.ERRAsync(cts);
        }

        /// <summary>
        /// 机器人使能
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_SvonAsync( CancellationToken cts = default(CancellationToken) )
        {
            await _robot.SVONAsync(cts);
        }

        /// <summary>
        /// 机器人解使能
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_SvofAsync( CancellationToken cts = default(CancellationToken) )
        {
            await _robot.SVOFAsync(cts);
        }

        /// <summary>
        /// 机器人回原点
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_HomeAsync( CancellationToken cts = default(CancellationToken) )
        {
            await _robot.HOMAsync(cts);
        }

        public static async Task<string> SGRPAsync( string sta, CancellationToken cts = default(CancellationToken) )
        {
            return await _robot.SGRPAsync(sta, cts);
        }

        /// <summary>
        /// 读取电缸当前位置
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_RLIPAsync( CancellationToken cts = default(CancellationToken) )
        {
            return await _robot.REFPAsync(cts);
        }

        /// <summary>
        /// Fork翻转
        /// </summary>
        /// <param name="saftSta"></param>
        /// <param name="mode"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_FLIPAsync( string saftSta, string mode, CancellationToken cts = default(CancellationToken) )
        {
            await _robot.FLIPAsync(saftSta, mode, cts);
        }

        /// <summary>
        /// 机器人下吸式取片
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_GetAsync( Station station, string slot, CancellationToken cts )
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.GETAsync(sta, slot, cts);
        }

        /// <summary>
        /// 带判断后下吸式取片
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_GetwAsync( StationName.Station station, string slot, CancellationToken cts )
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.GETWAsync(sta, slot, cts);
        }

        /// <summary>
        /// 机器人上吸式取片
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_GetDAsync( StationName.Station station, string slot, CancellationToken cts )
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.GETDAsync(sta, slot, cts);
        }

        /// <summary>
        /// 机器人下吸式放片
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_PutAsync( StationName.Station station, string slot, CancellationToken cts )
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.PUTAsync(sta, slot, cts);
        }

        /// <summary>
        /// 机器人移动至站点位置
        /// </summary>
        /// <param name="station"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_MoveAsync( StationName.Station station, CancellationToken cts )
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.MTCSAsync(sta, cts);
            await Task.Delay(3000);
            await _robot.MOVAAsync("R", "0");
           
        }

        /// <summary>
        /// 机器人上吸式放片
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task Robot_PutDAsync( StationName.Station station, string slot, CancellationToken cts )
        {
            string sta = StationName.GetAlias(station).ToUpper();
            await _robot.PUTDAsync(sta, slot, cts);
        }

        /// <summary>
        /// 机器人扫片
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_MappingAsync( StationName.Station station, CancellationToken cts = default )
        {
            string sta = StationName.GetAlias(station).ToLower();
            await _robot.MAPAsync(sta, cts);
            return await _robot.RSRAsync(cts);
        }

        /// <summary>
        /// 机器人读取扫片结果
        /// </summary>
        /// <param name="station"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_RSRAsync( StationName.Station station, CancellationToken cts = default )
        {
            string sta = StationName.GetAlias(station).ToLower();
            await _robot.RSRAsync(cts);
            return await _robot.RSRAsync(cts);
        }

        /// <summary>
        /// 控制真空开关
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_VacSetAsync( CancellationToken cts = default )
        {
            return await _robot.OUTPAsync(1, 2);
        }

        /// <summary>
        /// 设定机器人速度
        /// </summary>
        /// <param name="spd"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_SSPPAsync( int spd, int forkslot, CancellationToken cts = default )
        {
            return await _robot.SSPPAsync(spd, forkslot, cts);
        }

        /// <summary>
        /// 读取R轴当前位置
        /// </summary>
        /// <returns></returns>
        public static async Task<int> Robot_RCPAsync_R()
        {
            string[] list = await _robot.RCPAsync();
            return list[1].ToInt();
        }

        /// <summary>
        /// 机器人相对移动
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_MovRAsync( string axis, string pos, CancellationToken cts = default )
        {
            return await _robot.MOVRAsync(axis, pos, cts);
        }

        public static async Task<string> Robot_MovSefteyStaAsync( CancellationToken cts = default )
        {
            return await _robot.MTCSAsync("Z", cts);
        }

        /// <summary>
        /// 机器人停止动作
        /// </summary>
        /// <returns></returns>
        public static async Task<string> Robot_StopAsync()
        {
            return await _robot.STOPAsync();
        }

        /// <summary>
        /// 机器人保存站点
        /// </summary>
        /// <param name="staName"></param>
        /// <param name="getputAndMap"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<string> Robot_SaveStation( Station staName, string getputAndMap, CancellationToken cts = default )
        {
            if ( getputAndMap == "GetPut" )
            {
                string sta = StationName.GetAlias(staName).ToUpper();
                return await _robot.SCPAsync(sta, cts);
            }
            else if ( getputAndMap == "Map" )
            {
                string sta = StationName.GetAlias(staName).ToLower();
                return await _robot.SCSPAsync(sta, cts);
            }
            else
            {
                return "Error";
            }
        }

        /// <summary>
        /// 读取当前轴位置
        /// </summary>
        /// <returns></returns>
        public static async Task<string[]> Robot_ReadPos()
        {
            return await _robot.RCPAsync();
        }
    }
}