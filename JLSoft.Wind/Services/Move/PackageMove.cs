using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JLSoft.Wind.Adapter;
using JLSoft.Wind.Database;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Logs;
using Sunny.UI;
using Sunny.UI.Win32;
using static JLSoft.Wind.Database.StationName;
using static JLSoft.Wind.Services.Leisai_Axis;

namespace JLSoft.Wind.Services
{
    public class PackageMove
    {
        private static Station NowStation = StationName.Station.G1;

        /// <summary>
        /// 加载工件类型参数
        /// </summary>
        /// <param name="cts"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> LoadSettingAsync( CancellationToken cts )
        {
            try
            {
                await HWAligner.Aligner_Setting(cts);
                //await AngleT.AngleTInitAsync(cts);
                return true;
            }
            catch ( Exception ex )
            {
                return false;
                throw new Exception($"Aligner Setting Error:{ex}");
            }
        }

        /// <summary>
        /// LoadPort加载
        /// </summary>
        /// <param name="station"></param>
        /// <param name="velstr"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> LoadMapAsync( Station station, CancellationToken cts )
        {
            try
            {
                int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
                if ( robotR_Pos != 0 )
                {
                    //Form1.SystemInfo = $"<Robot> [Error] Axis移动失败，请检查Robot R轴是否在缩回状态！";
                    LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！", Sunny.UI.LogLevel.Error, "Robot.Main");
                    return false;
                }
                if ( MainForm.CurrentWaferType == "Wafer" )
                {
                    if ( station == Station.LoadPort1 )
                    {
                        await HWLoadPort_1.LoadPort1_MapLoadAsync();
                        await HWLoadPort_1.LP1ReadMappingAsync();
                    }
                    else
                    {
                        await HWLoadPort_2.LoadPort2_MapLoadAsync();
                        await HWLoadPort_2.LP2ReadMappingAsync();
                    }
                }
                else
                {
                    if ( station == Station.LoadPort1 )
                    {
                        await HWLoadPort_1.LoadPort1_LoadAsync();
                        Positions position = ConfigService.FindPosition("LP1");
                        await MapAsync(station, position);
                        await HWRobot.Robot_RSRAsync(station);
                    }
                    else
                    {
                        await HWLoadPort_2.LoadPort2_LoadAsync();

                        Positions position = ConfigService.FindPosition("LP2");
                        await MapAsync(station, position);
                    }
                }
                return true;
            }
            catch ( Exception ex )
            {
                //Form1.SystemInfo = $"<LoadPort> [Error] {ex.Message}";
                LogManager.Log($"LoadPort加载异常: {ex.Message}" , Sunny.UI.LogLevel.Error , "LoadPort.Main");
                return false;
            }
        }

        /// <summary>
        /// LoadPort卸载
        /// </summary>
        /// <param name="station"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> UnLoadAsync( Station station, CancellationToken cts )
        {
            try
            {
                int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
                if ( robotR_Pos != 0 )
                {
                    LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！" , Sunny.UI.LogLevel.Error , "Robot.Main") ;
                    return false;
                }
                await HWLoadPort_1.LoadPort1_UnloadAsync();
                return true;
            }
            catch ( Exception ex )
            {
                LogManager.Log($"LoadPort卸载异常: {ex.Message}" , Sunny.UI.LogLevel.Error , "LoadPort.Main");
                return false;
            }
        }

        /// <summary>
        /// 3轴移动至指定站点Robot执行Map
        /// </summary>
        /// <param name="station"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> MapAsync( Station station, Positions post)
        {
            try
            {
                int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
                if ( robotR_Pos != 0 )
                {
                    //Form1.SystemInfo = $"<Robot> [Error] Axis移动失败，请检查Robot R轴是否在缩回状态！";
                    LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！", Sunny.UI.LogLevel.Error, "Robot.Main");
                    return false;
                }
                if ( NowStation != station )
                {
                    await HWRobot.Robot_MovSefteyStaAsync();
                }
                Leisai_Axis.SetProfileUnit((ushort) AxisName.X, 0, Leisai_Axis.Axis_X_Speed);
                Leisai_Axis.SetProfileUnit((ushort) AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                Leisai_Axis.SetProfileUnit((ushort) AxisName.Z, 0, Leisai_Axis.Axis_Z_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y轴已移动至安全位");

                var stateX = Leisai_Axis.Leisai_Pmov((ushort) AxisName.X, post.X, 1);
                var inpX = Axis_INP.AxisXINPAsync(post.X);

                var stateZ = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Z, post.Z, 1);
                var inpZ = Axis_INP.AxisZINPAsync(post.Z);
                await Task.WhenAll(stateZ, inpZ, stateX, inpX);
                Console.WriteLine("X/Z轴已移动至站点位");

                var stateY = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Y, post.Y, 1);
                var inpY = Axis_INP.AxisYINPAsync(post.Y);
                await Task.WhenAll(stateY, inpY);
                Console.WriteLine("Y轴已移动至站点位");


                string rsr = await HWRobot.Robot_MappingAsync(station);
                LoadPortAndRobot_Wafer_UI.Mapping_UI(station, rsr);

                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();

                NowStation = station;
                Console.WriteLine("Y轴已移动至安全位");
                return true;
            }
            catch ( Exception ex )
            { return false; }
        }

        /// <summary>
        /// 3轴移动至指定站点Robot执行Get
        /// </summary>
        /// <param name="station"></param>
        /// <param name="velstr"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> GetWaferAsync( Station station, string slot, CancellationToken cts , Positions post)
        {
            int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
            if ( robotR_Pos != 0 )
            {
                //Form1.SystemInfo = $"<Robot> [Error] Axis移动失败，请检查Robot R轴是否在缩回状态！";
                LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！", Sunny.UI.LogLevel.Error, "Robot.Main");
                return false;
            }
            if ( NowStation != station )
            {
                await HWRobot.Robot_MovSefteyStaAsync();
            }
            Leisai_Axis.SetProfileUnit((ushort) AxisName.X, 0, Leisai_Axis.Axis_X_Speed);
            Leisai_Axis.SetProfileUnit((ushort) AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
            Leisai_Axis.SetProfileUnit((ushort) AxisName.Z, 0, Leisai_Axis.Axis_Z_Speed);
            await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();

            Console.WriteLine("Y轴已移动至安全位");

            var stateX = Leisai_Axis.Leisai_Pmov((ushort) AxisName.X, post.X, 1);
            var inpX = Axis_INP.AxisXINPAsync(post.X);

            var stateZ = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Z, post.Z, 1);
            var inpZ = Axis_INP.AxisZINPAsync(post.Z);
            await Task.WhenAll(stateZ, inpZ, stateX, inpX);
            Console.WriteLine("X/Z轴已移动至站点位");

            var stateY = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Y, post.Y, 1);
            var inpY = Axis_INP.AxisYINPAsync(post.Y);
            await Task.WhenAll(stateY, inpY);
            Console.WriteLine("Y轴已移动至站点位");

            cts.ThrowIfCancellationRequested();
            await HWRobot.Robot_GetAsync(station, slot, cts);

            cts.ThrowIfCancellationRequested();
            if ( station == StationName.Station.LoadPort1 || station == StationName.Station.LoadPort2 )
            {
                LoadPortAndRobot_Wafer_UI.Cst_Wafer_UI(station, Convert.ToInt16(slot), "get");
            }
            cts.ThrowIfCancellationRequested();
            await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();

            NowStation = station;
            Console.WriteLine("Y轴已移动至安全位");
            return true;
        }

        /// <summary>
        /// 3轴移动至指定站点Robot执行Put
        /// </summary>
        /// <param name="station"></param>
        /// <param name="velstr"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> PutWaferAsync( Station station, string slot, CancellationToken cts, Positions positions )
        {
            int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
            if ( robotR_Pos != 0 )
            {
                //Form1.SystemInfo = $"<Robot> [Error] Axis移动失败，请检查Robot R轴是否在缩回状态！";
                LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！", Sunny.UI.LogLevel.Error, "Robot.Main");
                return false;
            }
            if ( NowStation != station )
            {
                await HWRobot.Robot_MovSefteyStaAsync();    //Y轴移动至安全位置
            }
            if ( station == Station.Aligner )
            {
                await HWAligner.Aligner_MoveToCenter(); //Aligner移动至中心
            }
            if ( station == Station.AngleT )
            {
                await AngleT.MTMAsync();    //AngleT移动至中心
            }
            Leisai_Axis.SetProfileUnit((ushort) AxisName.X, 0, Leisai_Axis.Axis_X_Speed);
            Leisai_Axis.SetProfileUnit((ushort) AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
            Leisai_Axis.SetProfileUnit((ushort) AxisName.Z, 0, Leisai_Axis.Axis_Z_Speed);
            await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
            Console.WriteLine("Y轴已移动至安全位");

            var stateX = Leisai_Axis.Leisai_Pmov((ushort) AxisName.X, positions.X, 1);
            var inpX = Axis_INP.AxisXINPAsync(positions.X);

            var stateZ = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Z, positions.Z, 1);
            var inpZ = Axis_INP.AxisZINPAsync(positions.Z);
            await Task.WhenAll(stateZ, inpZ, stateX, inpX);
            Console.WriteLine("X/Z轴已移动至站点位");

            var stateY = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Y, positions.Y, 1);
            var inpY = Axis_INP.AxisYINPAsync(positions.Y);
            await Task.WhenAll(stateY, inpY);
            Console.WriteLine("Y/Z轴已移动至站点位");

            cts.ThrowIfCancellationRequested();
            await HWRobot.Robot_PutAsync(station, slot, cts);

            cts.ThrowIfCancellationRequested();
            if ( station == StationName.Station.LoadPort1 || station == StationName.Station.LoadPort2 )
            {
                LoadPortAndRobot_Wafer_UI.Cst_Wafer_UI(station, Convert.ToInt16(slot), "put");
            }
            cts.ThrowIfCancellationRequested();
            await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();

            Console.WriteLine("Y轴已移动至安全位");
            NowStation = station;
            return true;
        }

        /// <summary>
        /// 3轴移动至指定站点Robot执行MTCS
        /// </summary>
        /// <param name="station"></param>
        /// <param name="slot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async Task<bool> MTCSAsync( Station station, CancellationToken cts, Positions positions)
        {
            int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
            if ( robotR_Pos != 0 )
            {
                //Form1.SystemInfo = $"<Robot> [Error] Axis移动失败，请检查Robot R轴是否在缩回状态！";
                LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！", Sunny.UI.LogLevel.Error, "Robot.Main");
                return false;
            }
            if ( NowStation != station )
            {
                await HWRobot.Robot_MovSefteyStaAsync();    //Y轴移动至安全位置
            }
            Leisai_Axis.SetProfileUnit((ushort) AxisName.X, 0, Leisai_Axis.Axis_X_Speed);
            Leisai_Axis.SetProfileUnit((ushort) AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
            Leisai_Axis.SetProfileUnit((ushort) AxisName.Z, 0, Leisai_Axis.Axis_Z_Speed);
            await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
            Console.WriteLine("Y轴已移动至安全位");

            var stateX = Leisai_Axis.Leisai_Pmov((ushort) AxisName.X, positions.X, 1);
            var inpX = Axis_INP.AxisXINPAsync(positions.X);

            var stateZ = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Z, positions.Z, 1);
            var inpZ = Axis_INP.AxisZINPAsync(positions.Z);
            await Task.WhenAll(stateZ, inpZ, stateX, inpX);
            Console.WriteLine("X/Z轴已移动至站点位");

            var stateY = Leisai_Axis.Leisai_Pmov((ushort) AxisName.Y, positions.Y, 1);
            var inpY = Axis_INP.AxisYINPAsync(positions.Y);
            await Task.WhenAll(stateY, inpY);
            Console.WriteLine("Y/Z轴已移动至站点位");

            cts.ThrowIfCancellationRequested();
            await HWRobot.Robot_MoveAsync(station, cts);

            cts.ThrowIfCancellationRequested();
            await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();

            Console.WriteLine("Y轴已移动至安全位");
            NowStation = station;
            return true;
        }

        /// <summary>
        /// 移动至预备取片位置
        /// </summary>
        /// <param 3轴坐标="positions"></param>
        /// <param Robot站点名="sta"></param>
        /// <param 槽数="slot"></param>
        /// <returns></returns>
        public static async Task MoveGetReadyPos(Positions positions ,string sta, string slot,bool isGet)
        {
            int robotR_Pos = await HWRobot.Robot_RCPAsync_R();
            if (robotR_Pos != 0)
            {
                //Form1.SystemInfo = $"<Robot> [Error] Axis移动失败，请检查Robot R轴是否在缩回状态！";
                LogManager.Log($"Axis移动失败，请检查Robot R轴是否在缩回状态！", Sunny.UI.LogLevel.Error, "Robot.Main");
                return;
            }

            Leisai_Axis.SetProfileUnit((ushort)AxisName.X, 0, Leisai_Axis.Axis_X_Speed);
            Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
            Leisai_Axis.SetProfileUnit((ushort)AxisName.Z, 0, Leisai_Axis.Axis_Z_Speed);
            var sta1 = StationName.ChangeStaName(sta);
            if (NowStation != sta1)
            {
                await HWRobot.Robot_MovSefteyStaAsync();
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                Console.WriteLine("Y轴已移动至安全位");
            }

            var stateX = Leisai_Axis.Leisai_Pmov((ushort)AxisName.X, positions.X, 1);
            var inpX = Axis_INP.AxisXINPAsync(positions.X);

            var stateZ = Leisai_Axis.Leisai_Pmov((ushort)AxisName.Z, positions.Z, 1);
            var inpZ = Axis_INP.AxisZINPAsync(positions.Z);
            await Task.WhenAll(stateZ, inpZ, stateX, inpX);
            Console.WriteLine("X/Z轴已移动至站点位");

            var stateY = Leisai_Axis.Leisai_Pmov((ushort)AxisName.Y, positions.Y, 1);
            var inpY = Axis_INP.AxisYINPAsync(positions.Y);
            await Task.WhenAll(stateY, inpY);
            Console.WriteLine("Y轴已移动至站点位");
            if (slot == null || slot == "")
            {
                slot = "1";
            }
            if (isGet)
            {
                await HWRobot._robot.GETSTAsync(sta, slot);
            }
            else
            {
                await HWRobot._robot.PUTSTAsync(sta, slot);
            }
        }

        /// <summary>
        /// 单步取放片
        /// </summary>
        /// <param 3轴坐标="positions"></param>
        /// <param Robot站点名="sta"></param>
        /// <param 槽数="slot"></param>
        /// <param name="isGet"></param>
        /// <returns></returns>
        public static async Task OneKeyGetPut(Positions positions, string sta, string slot, bool isGet)
        {
            var flag = Axis_INP.AxisPosNowAsync(positions.X, positions.Y, positions.Z);
            if (!flag)
            {
                Leisai_Axis.SetProfileUnit((ushort)AxisName.X, 0, Leisai_Axis.Axis_X_Speed);
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Y, 0, Leisai_Axis.Axis_Y_Speed);
                Leisai_Axis.SetProfileUnit((ushort)AxisName.Z, 0, Leisai_Axis.Axis_Z_Speed);
                await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();

                Console.WriteLine("Y轴已移动至安全位");

                var stateX = Leisai_Axis.Leisai_Pmov((ushort)AxisName.X, positions.X, 1);
                var inpX = Axis_INP.AxisXINPAsync(positions.X);

                var stateZ = Leisai_Axis.Leisai_Pmov((ushort)AxisName.Z, positions.Z, 1);
                var inpZ = Axis_INP.AxisZINPAsync(positions.Z);
                await Task.WhenAll(stateZ, inpZ, stateX, inpX);
                Console.WriteLine("X/Z轴已移动至站点位");

                var stateY = Leisai_Axis.Leisai_Pmov((ushort)AxisName.Y, positions.Y, 1);
                var inpY = Axis_INP.AxisYINPAsync(positions.Y);
                await Task.WhenAll(stateY, inpY);
                Console.WriteLine("Y轴已移动至站点位");
            }
            if (slot == null || slot == "")
            {
                slot = "1";
            }
            
            if (isGet)
            {
                await HWRobot._robot.GETAsync(sta, slot);
                //var states = HWLoadPort_2.MappingStateLp2;
                //states = states.Substring(0, slot.ToInt() - 1) + 0 + states.Substring(slot.ToInt());
            }
            else
            {
                await HWRobot._robot.PUTAsync(sta, slot);

            }
            // 更新UI（WinForms环境下确保在UI线程执行）
            if (sta == "F" )
            {
                var states = HWLoadPort_1.MappingStateLp1;
                //states = states.Substring(0, slot.ToInt() - 1) + (isGet ? "0" : "1") + states.Substring(slot.ToInt());
                StringBuilder str = new StringBuilder(states);
                var i = Convert.ToInt16(slot);
                str[i-1] = (isGet ? '0' : '1');
                HWLoadPort_1.MappingStateLp1 = str.ToString();
                EventAggregator.RequestLoadPortColorUpdate("LoadPort1", HWLoadPort_1.MappingStateLp1);
            }
            else if(sta == "G")
            {
                var states = HWLoadPort_2.MappingStateLp2;
                StringBuilder str = new StringBuilder(states);
                var i = Convert.ToInt16(slot);
                str[i - 1] = (isGet ? '0' : '1');
                HWLoadPort_2.MappingStateLp2 = str.ToString();
                EventAggregator.RequestLoadPortColorUpdate("LoadPort2", HWLoadPort_2.MappingStateLp2);
            }
            
            
        }
    }
    
}