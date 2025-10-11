using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Services;
using static JLSoft.Wind.Database.StationName;

namespace JLSoft.Wind
{
    public class TeachServer
    {
        #region 配置常量与状态变量（独立隔离）

        private const int MaxClientsPerServer = 2;
        private const int StatusSendInterval = 100;
        private const int ListenRestartDelay = 1000;

        // 命令服务
        private static int _cmdClientCount;

        private static readonly object _cmdCountLock = new object();
        private static CancellationTokenSource _cmdCts;
        private static TcpListener _cmdListener;

        // 状态服务
        private const int PortStatus = 8602;

        public static int _statusClientCount;
        private static readonly object _statusCountLock = new object();
        private static CancellationTokenSource _statusCts;
        private static TcpListener _statusListener;

        // 全局状态
        public static bool IsTeachServerRunning => _cmdCts != null && !_cmdCts.Token.IsCancellationRequested;

        public static ushort AxisXState1, AxisYState1, AxisZState1;
        public static bool Axis_X_TServer_State, Axis_Y_TServer_State, Axis_Z_TServer_State;
        private static IPAddress _localIP => IPAddress.Parse(ConfigService.GetTeachServerIp());
        private static int _cmdPort => ConfigService.GetTeachServerPort();

        #endregion 配置常量与状态变量（独立隔离）

        #region 服务启动入口

        public static async Task<bool> StartCmdServerAsync()
        {
            StopCmdServer();
            _cmdCts = new CancellationTokenSource();
            _cmdClientCount = 0;

            Console.WriteLine($"=== 命令服务启动中，目标端口：{_cmdPort} ===");
            await RunCmdListenLoopAsync(_cmdCts.Token);
            return true;
        }

        public static async Task StartStatusServerAsync()
        {
            StopStatusServer();
            _statusCts = new CancellationTokenSource();
            _statusClientCount = 0;

            Console.WriteLine($"=== 状态服务启动中，固定端口：{PortStatus} ===");
            await RunStatusListenLoopAsync(_statusCts.Token);
        }

        public static void StopAllServers()
        {
            StopCmdServer();
            StopStatusServer();
            Console.WriteLine("=== 所有TCP服务已停止 ===");
        }

        #endregion 服务启动入口

        #region 命令服务核心逻辑

        private static async Task RunCmdListenLoopAsync( CancellationToken token )
        {
            while ( !token.IsCancellationRequested )
            {
                try
                {
                    _cmdListener = new TcpListener(_localIP, _cmdPort);
                    _cmdListener.Start();
                    Console.WriteLine($"命令服务监听成功：{_localIP}:{_cmdPort}，最大客户端：{MaxClientsPerServer}");

                    while ( _cmdClientCount < MaxClientsPerServer && !token.IsCancellationRequested )
                    {
                        TcpClient client = await _cmdListener.AcceptTcpClientAsync();
                        Interlocked.Increment(ref _cmdClientCount);

                        string clientIp = client.Client.RemoteEndPoint.ToString();
                        Console.WriteLine($"命令服务新连接：{clientIp}，当前连接数：{_cmdClientCount}");

                        await HandleCmdClientAsync(client, token);
                    }
                }
                catch ( OperationCanceledException )
                {
                    Console.WriteLine("命令服务监听已主动取消");
                    break;
                }
                catch ( SocketException ex )
                {
                    if ( token.IsCancellationRequested ) break;
                    Console.WriteLine($"命令服务监听异常：{ex.Message}，{ListenRestartDelay}ms后重启");
                }
                catch ( Exception ex )
                {
                    if ( token.IsCancellationRequested ) break;
                    Console.WriteLine($"命令服务未知异常：{ex.Message}，{ListenRestartDelay}ms后重启");
                }
                finally
                {
                    _cmdListener?.Stop();
                    if ( !token.IsCancellationRequested )
                    {
                        await Task.Delay(ListenRestartDelay, token);
                    }
                }
            }
        }

        private static async Task HandleCmdClientAsync( TcpClient client, CancellationToken token )
        {
            string clientIp = client.Client.RemoteEndPoint.ToString();
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                stream.ReadTimeout = 5000;
                byte[] buffer = new byte[1024];

                while ( !token.IsCancellationRequested )
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    }
                    catch ( TimeoutException )
                    {
                        continue;
                    }

                    if ( bytesRead == 0 )
                    {
                        Console.WriteLine($"命令服务客户端断开（主动关闭）：{clientIp}");
                        break;
                    }

                    string cmd = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim('\r', '\n');
                    Console.WriteLine($"命令服务收到指令：{clientIp} -> {cmd}");

                    await Readdata(cmd);

                    byte[] response = Encoding.UTF8.GetBytes(">\r\n");
                    await stream.WriteAsync(response, 0, response.Length, token);

                    if ( string.Equals(cmd, "!disconnect", StringComparison.OrdinalIgnoreCase) )
                    {
                        Console.WriteLine($"命令服务客户端请求断开：{clientIp}");
                        break;
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                Console.WriteLine($"命令服务客户端处理已取消：{clientIp}");
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"命令服务客户端通信异常：{clientIp} -> {ex.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref _cmdClientCount);
                stream?.Dispose();
                client.Dispose();
                Console.WriteLine($"命令服务客户端资源释放：{clientIp}，剩余连接数：{_cmdClientCount}");
            }
        }

        private static void StopCmdServer()
        {
            _cmdCts?.Cancel();
            _cmdCts?.Dispose();
            _cmdListener?.Stop();
            _cmdClientCount = 0;
            Console.WriteLine("命令服务已停止");
        }

        #endregion 命令服务核心逻辑

        #region 状态服务核心逻辑

        private static async Task RunStatusListenLoopAsync( CancellationToken token )
        {
            while ( !token.IsCancellationRequested )
            {
                try
                {
                    _statusListener = new TcpListener(_localIP, PortStatus);
                    _statusListener.Start();
                    Console.WriteLine($"状态服务监听成功：{_localIP}:{PortStatus}，最大客户端：{MaxClientsPerServer}");

                    while ( _statusClientCount < MaxClientsPerServer && !token.IsCancellationRequested )
                    {
                        TcpClient client = await _statusListener.AcceptTcpClientAsync();
                        Interlocked.Increment(ref _statusClientCount);

                        string clientIp = client.Client.RemoteEndPoint.ToString();
                        Console.WriteLine($"状态服务新连接：{clientIp}，当前连接数：{_statusClientCount}");

                        _ = HandleStatusClientAsync(client, token);
                    }
                }
                catch ( OperationCanceledException )
                {
                    Console.WriteLine("状态服务监听已主动取消");
                    break;
                }
                catch ( SocketException ex )
                {
                    if ( token.IsCancellationRequested ) break;
                    Console.WriteLine($"状态服务监听异常：{ex.Message}，{ListenRestartDelay}ms后重启");
                }
                catch ( Exception ex )
                {
                    if ( token.IsCancellationRequested ) break;
                    Console.WriteLine($"状态服务未知异常：{ex.Message}，{ListenRestartDelay}ms后重启");
                }
                finally
                {
                    _statusListener?.Stop();
                    if ( !token.IsCancellationRequested )
                    {
                        await Task.Delay(ListenRestartDelay, token);
                    }
                }
            }
        }

        private static async Task HandleStatusClientAsync( TcpClient client, CancellationToken token )
        {
            string clientIp = client.Client.RemoteEndPoint.ToString();
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                stream.WriteTimeout = 5000;
                byte[] reqBuffer = new byte[256];

                while ( !token.IsCancellationRequested )
                {
                    // 读取客户端请求
                    if ( stream.DataAvailable )
                    {
                        int reqLen = await stream.ReadAsync(reqBuffer, 0, reqBuffer.Length, token);
                        if ( reqLen > 0 )
                        {
                            string req = Encoding.UTF8.GetString(reqBuffer, 0, reqLen).Trim();
                            //Console.WriteLine($"状态服务收到请求：{clientIp} -> {req}");
                        }
                    }

                    // 获取状态并推送
                    string statusData = GetDeviceStatusData();
                    byte[] statusBytes = Encoding.UTF8.GetBytes($"{statusData}\r\n");

                    try
                    {
                        await stream.WriteAsync(statusBytes, 0, statusBytes.Length, token);
                        await stream.FlushAsync(token);
                    }
                    catch ( IOException ex )
                    {
                        Console.WriteLine($"状态服务推送失败：{clientIp} -> {ex.Message}");
                        break;
                    }

                    await Task.Delay(StatusSendInterval, token);
                }
            }
            catch ( OperationCanceledException )
            {
                Console.WriteLine($"状态服务客户端处理已取消：{clientIp}");
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"状态服务客户端通信异常：{clientIp} -> {ex.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref _statusClientCount);
                stream?.Dispose();
                client.Dispose();
                Console.WriteLine($"状态服务客户端资源释放：{clientIp}，剩余连接数：{_statusClientCount}");
            }
        }

        private static void StopStatusServer()
        {
            _statusCts?.Cancel();
            _statusCts?.Dispose();
            _statusListener?.Stop();
            _statusClientCount = 0;
            Console.WriteLine("状态服务已停止");
        }

        #endregion 状态服务核心逻辑

        #region 业务逻辑

        private static async Task Readdata( string cmd )
        {
            if ( string.IsNullOrEmpty(cmd) ) return;

            switch ( cmd )
            {
                case "AllStop":
                    Leisai_Axis.Leisai_Stop((ushort)Leisai_Axis.AxisName.X, 0);
                    Leisai_Axis.Leisai_Stop((ushort)Leisai_Axis.AxisName.Y, 0);
                    Leisai_Axis.Leisai_Stop((ushort)Leisai_Axis.AxisName.Z, 0);
                    break;

                case "RRems":
                    await HWRobot.Robot_RemsAsync();
                    break;

                case "RSvon":
                    await HWRobot.Robot_SvonAsync();
                    break;

                case "RSvof":
                    await HWRobot.Robot_SvofAsync();
                    break;

                case "RHome":
                    await HWRobot.Robot_HomeAsync();
                    break;

                case "VacSet":
                    await HWRobot.Robot_VacSetAsync();
                    break;

                case "ForkSet":
                    string forkPos = HWRobot.Robot_RLIPAsync().Result.Replace("\r\n", "");
                    await Task.Delay(1000);
                    if ( forkPos == "1" || forkPos == "5" )
                    {
                        await HWRobot.Robot_FLIPAsync("Z", "2");
                    }
                    else if ( forkPos == "2" )
                    {
                        await HWRobot.Robot_FLIPAsync("Z", "1");
                    }
                    break;

                // 轴控制
                case "AStop":
                    Leisai_Axis.Leisai_Stop((ushort)Leisai_Axis.AxisName.X, 0);
                    Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Y, 0);
                    Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Z, 0);
                    break;

                case "AClrErr":
                    Leisai_Axis.Leisai_ClearErrCode((ushort) Leisai_Axis.AxisName.X);
                    Leisai_Axis.Leisai_ClearErrCode((ushort) Leisai_Axis.AxisName.Y);
                    Leisai_Axis.Leisai_ClearErrCode((ushort) Leisai_Axis.AxisName.Z);
                    break;

                case "ASvon":
                    Leisai_Axis.SetAxisEnable((ushort) Leisai_Axis.AxisName.X);
                    Leisai_Axis.SetAxisEnable((ushort)  Leisai_Axis.AxisName.Y);
                    Leisai_Axis.SetAxisEnable((ushort) Leisai_Axis.AxisName.Z);
                    break;

                case "ASvof":
                    Leisai_Axis.SetAxisDisable((ushort) Leisai_Axis.AxisName.X);
                    Leisai_Axis.SetAxisDisable((ushort) Leisai_Axis.AxisName.Y);
                    Leisai_Axis.SetAxisDisable((ushort) Leisai_Axis.AxisName.Z);
                    break;

                default:
                    HandleAxisJogCmd(cmd);
                    await HandleAxisSaveMoveCmd(cmd);
                    await HandleRobotSpdGetPutCmd(cmd);
                    if ( cmd.Contains("RStop") )
                    {
                        _ = HWRobot.Robot_StopAsync();
                    }
                    _ = RobotJogCmd(cmd);

                    break;
            }
        }

        private static void HandleAxisJogCmd( string cmd )
        {
            if ( cmd.StartsWith("AJog", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');
                if ( parts.Length < 2 ) return;

                double velocity;
                if ( !double.TryParse(parts[1], out velocity) )
                {
                    Console.WriteLine($"轴Jog指令参数错误：{cmd}（速度需为数字）");
                    return;
                }

                if ( cmd.Contains("X+") )
                {
                    Leisai_Axis.SetProfileUnit((ushort) Leisai_Axis.AxisName.X, 0, velocity);
                    Leisai_Axis.Leisai_Vmov((ushort) Leisai_Axis.AxisName.X, 1);
                }
                else if ( cmd.Contains("X-") )
                {
                    Leisai_Axis.SetProfileUnit((ushort) Leisai_Axis.AxisName.X, 0, velocity);
                    Leisai_Axis.Leisai_Vmov((ushort) Leisai_Axis.AxisName.X, 0);
                }
                else if ( cmd.Contains("Y+") )
                {
                    Leisai_Axis.SetProfileUnit((ushort) Leisai_Axis.AxisName.Y, 0, velocity);
                    Leisai_Axis.Leisai_Vmov((ushort) Leisai_Axis.AxisName.Y, 1);
                }
                else if ( cmd.Contains("Y-") )
                {
                    Leisai_Axis.SetProfileUnit((ushort) Leisai_Axis.AxisName.Y, 0, velocity);
                    Leisai_Axis.Leisai_Vmov((ushort) Leisai_Axis.AxisName.Y, 0);
                }
                else if ( cmd.Contains("Z+") )
                {
                    Leisai_Axis.SetProfileUnit((ushort) Leisai_Axis.AxisName.Z, 0, velocity);
                    Leisai_Axis.Leisai_Vmov((ushort) Leisai_Axis.AxisName.Z, 1);
                }
                else if ( cmd.Contains("Z-") )
                {
                    Leisai_Axis.SetProfileUnit((ushort) Leisai_Axis.AxisName.Z, 0, velocity);
                    Leisai_Axis.Leisai_Vmov((ushort) Leisai_Axis.AxisName.Z, 0);
                }
            }
        }

        private static async Task HandleAxisSaveMoveCmd( string cmd )
        {
            if ( cmd.StartsWith("AxisSavPoint", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');
                if ( parts.Length < 2 ) return;

                string staStr = parts[1];
                Station station;
                if ( !System.Enum.TryParse<Station>(staStr, out station) )
                {
                    Console.WriteLine($"轴点位保存：站点格式错误：{staStr}");
                    return;
                }

                //if ( Form1._instance.InvokeRequired )
                //{
                //    Form1._instance.Invoke(new Action(() => SaveAxisStation(station)));
                //}
                //else
                //{
                //    SaveAxisStation(station);
                //}
            }
            else if ( cmd.StartsWith("AxisMovSta", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');
                if ( parts.Length < 5 ) return;

                Station station;
                double velX, velY, velZ;
                if ( !System.Enum.TryParse<Station>(parts[1], out station) ||
                    !double.TryParse(parts[2], out velX) ||
                    !double.TryParse(parts[3], out velY) ||
                    !double.TryParse(parts[4], out velZ) )
                {
                    Console.WriteLine($"轴移动指令参数错误：{cmd}");
                    return;
                }

                //await Form1.AxisMovStation(station.ToString(), velX, velY, velZ);
            }
        }

        private static async Task RobotJogCmd( string cmd )
        {
            var parts = cmd.Split(',');
            string spd = parts[1].ToString();
            double d = 1;
            if ( spd == "Low" )
            {
                d = 10;
            }
            if ( spd == "Med" )
            {
                d = 2;
            }
            if ( spd == "High" )
            {
                d = 1;
            }

            string axisT = ( 10000 / d ).ToString("f0");
            string axisR = ( 75000 / d ).ToString("f0");
            string axisZ = ( 50000 / d ).ToString("f0");

            // 机器人Jog控制
            if ( cmd.Contains("JogT+") )
            {
                _ = HWRobot.Robot_MovRAsync("T", axisT);
            }
            if ( cmd.Contains("JogT-") )
            {
                _ = HWRobot.Robot_MovRAsync("T", "-" + axisT);
            }
            if ( cmd.Contains("JogR+") )
            {
                _ = HWRobot.Robot_MovRAsync("R", axisR);
            }
            if ( cmd.Contains("JogR-") )
            {
                _ = HWRobot.Robot_MovRAsync("R", "-" + axisR);
            }
            if ( cmd.Contains("JogZ+") )
            {
                _ = HWRobot.Robot_MovRAsync("Z", axisZ);
            }
            if ( cmd.Contains("JogZ-") )
            {
                _ = HWRobot.Robot_MovRAsync("Z", "-" + axisZ);
            }
        }

        private static async Task HandleRobotSpdGetPutCmd( string cmd )
        {
            if ( cmd.StartsWith("Sspp", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');

                if ( parts.Length >= 2 && int.TryParse(parts[1], out int robotSpd) )
                {
                    if ( robotSpd > 51 )
                    {
                        robotSpd = 50;
                    }
                    await HWRobot.Robot_SSPPAsync(robotSpd, 1);
                }
            }
            else if ( cmd.StartsWith("Get", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');
                Station station;
                if ( parts.Length >= 3 && System.Enum.TryParse<Station>(parts[1], out station) )
                {
                    await HWRobot.Robot_GetAsync(station, parts[2], CancellationToken.None);
                }
            }
            else if ( cmd.StartsWith("Put", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');
                Station station;
                if ( parts.Length >= 3 && System.Enum.TryParse<Station>(parts[1], out station) )
                {
                    await HWRobot.Robot_PutAsync(station, parts[2], CancellationToken.None);
                }
            }
            else if ( cmd.StartsWith("SavSta", StringComparison.OrdinalIgnoreCase) )
            {
                var parts = cmd.Split(',');
                Station station;
                if ( parts.Length >= 2 && System.Enum.TryParse<Station>(parts[1], out station) )
                {
                    await HWRobot.Robot_SaveStation(station, parts[2], CancellationToken.None);
                }
            }
        }

        private static void SaveAxisStation( Station station )
        {
            try
            {
                double x = double.Parse(Axis_INP.X_pos.ToString("F3"));
                double y = double.Parse(Axis_INP.Y_pos.ToString("F3"));
                double z = double.Parse(Axis_INP.Z_pos.ToString("F3"));

                Console.WriteLine($"轴点位保存成功：{station} -> X:{x}, Y:{y}, Z:{z}");
            }
            catch ( FormatException )
            {
                MessageBox.Show("轴位置格式错误，请输入有效数字", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch ( Exception ex )
            {
                MessageBox.Show($"轴点位保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetDeviceStatusData()
        {
            try
            {
                string axisXpos = "0", axisYpos = "0", axisZpos = "0";
                var x = Leisai_Axis.Leisai_GetPosition(0);
                var y =Leisai_Axis.Leisai_GetPosition(1);
                var z = Leisai_Axis.Leisai_GetPosition(2);
                axisXpos = x.ToString("F3");
                axisYpos = y.ToString("F3");
                axisZpos = z.ToString("F3");

                ushort axisXState = 0, axisYState = 0, axisZState = 0;
                string axisXInfo = Leisai_Axis.Leisai_GetAxisStatus((ushort) Leisai_Axis.AxisName.X, ref axisXState);
                string axisYInfo = Leisai_Axis.Leisai_GetAxisStatus((ushort) Leisai_Axis.AxisName.Y, ref axisYState);
                string axisZInfo = Leisai_Axis.Leisai_GetAxisStatus((ushort) Leisai_Axis.AxisName.Z, ref axisZState);

                Leisai_Axis.Leisai_CheckDone((ushort) Leisai_Axis.AxisName.X);
                Leisai_Axis.Leisai_CheckDone((ushort) Leisai_Axis.AxisName.Y);
                Leisai_Axis.Leisai_CheckDone((ushort) Leisai_Axis.AxisName.Z);

                string axisINPReady = ( !Axis_X_TServer_State || !Axis_Y_TServer_State || !Axis_Z_TServer_State )
                    ? $"{Axis_X_TServer_State},{Axis_Y_TServer_State},{Axis_Z_TServer_State}"
                    : $"{Axis_INP.Axis_X_INP},{Axis_INP.Axis_Y_INP},{Axis_INP.Axis_Z_INP}";

                return $"{axisXpos},{axisYpos},{axisZpos},{axisXState},{axisYState},{axisZState},{axisXInfo},{axisYInfo},{axisZInfo},{axisINPReady}";
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"状态数据获取失败：{ex.Message}");
                return "0,0,0,0,0,0,Error,Error,Error,False,False,False";
            }
        }

        #endregion 业务逻辑
    }
}