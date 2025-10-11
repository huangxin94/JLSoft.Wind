using DMCE3000;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JLSoft.Wind.Services
{
    public class Leisai_Axis
    {
        public static Ileisai _leisai = new Leisai();
        private static ushort CardNo = 0;

        public static string EtherCat_Status;
        public static string X_Axis_Status;
        public static string Y_Axis_Status;
        public static string Z_Axis_Status;

        public static double Axis_X_Speed =500;
        public static double Axis_Y_Speed =300;
        public static double Axis_Z_Speed = 15;

        private static double _Axis_AccDec = 1.2;
        private static double _Axis_VelStop = 100;

        private static double AxisX_Y_Home_LowSpeed = 2;
        private static double AxisX_Y_Home_HighSpeed = 100;

        private static double AxisZ_Home_LowSpeed = 2;
        private static double AxisZ_Home_HighSpeed = 10;

        public const double AxisY_Safety_Pos = 300.000;

        /// <summary>
        /// 轴加减速时间
        /// </summary>
        public static double Axis_AccDec
        {
            get
            {
                return _Axis_AccDec;
            }
            set
            {
                if ( value <= 1 && value >= 0.05 )
                {
                    _Axis_AccDec = value;
                }
                else
                {
                    _Axis_AccDec = 0.5;
                }
            }
        }

        public static double Axis_VelStop
        {
            get
            {
                return _Axis_VelStop;
            }
            set
            {
                if ( value <= 1000 && value >= 10 )
                {
                    _Axis_VelStop = value;
                }
                else
                {
                    _Axis_VelStop = 100;
                }
            }
        }

        /// <summary>
        /// 轴枚举
        /// </summary>
        public enum AxisName : ushort
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        /// <summary>
        /// 初始化轴卡
        /// </summary>
        /// <returns></returns>
        public static void Leisai_Init()
        {
            _leisai.BoardInit();
             Leisai_Axis.Leisai_DownloadConifigFile(MainForm.AxisConfigFilePath);
        }

        public static void Leisai_DownloadConifigFile( string filePath )
        {
            _leisai.DownloadConfig(CardNo, filePath);
        }

        /// <summary>
        /// 关闭轴卡
        /// </summary>
        /// <returns></returns>
        public static void Leisai_Close()
        {
            _leisai.BoardClose();
        }

        /// <summary>
        /// 错误清除
        /// </summary>
        /// <param name="axis"></param>
        public static void Leisai_ClearErrCode( ushort axis )
        {
            _leisai.ClearAxisErrcode(CardNo, axis);
        }

        /// <summary>
        /// 获取EtherCat状态
        /// </summary>
        /// <returns></returns>
        public static string Leisai_Geterrcode()
        {
            ushort errcode = 0;
            _leisai.GetErrcode(CardNo, ref errcode);
            if ( errcode == 0 )
            {
                return "EtherCat总线正常。";
            }
            else
            {
                return "[Error]EtherCat总线出错。";
            }
        }

        /// <summary>
        /// 清除EtherCAT总线总线错误码
        /// </summary>
        public static void Leisai_nmcClearErrcode()
        {
            _leisai.SetNmcClearErrcode(CardNo);
        }

        /// <summary>
        /// 读取Axis状态并输出Code解析
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="axisStateMachine"></param>
        /// <returns></returns>
        public static string Leisai_GetAxisStatus( ushort axis, ref ushort axisStateMachine )
        {
            _leisai.GetAxisStateMachine(CardNo, axis, ref axisStateMachine);
            switch ( axisStateMachine )
            {
                case 0:
                    return "0:[Error]未启动状态";

                case 1:
                    return "1:启动禁止状态";

                case 2:
                    return "2:准备启动状态";

                case 3:
                    return "3:启动状态";

                case 4:
                    return "4:操作使能状态";

                case 5:
                    return "5:停止状态";

                case 6:
                    return "6:[Error]错误触发状态";

                case 7:
                    return "7:[Error]错误状态";

                default:
                    return "8:[Error]未知错误";
            }
        }

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static double Leisai_GetPosition( ushort axis )
        {
            return _leisai.GetPosition(CardNo, axis);
        }

        /// <summary>
        /// 获取当前轴编码器反馈
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static double Leisai_GetEncoder( ushort axis )
        {
            double pos = 0;
            _leisai.GetEncoder(CardNo, axis, ref pos);
            return pos;
        }

        /// <summary>
        /// 获取Axis当前在状态：Run/Ready
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static short Leisai_CheckDone( ushort axis )
        {
            return _leisai.CheckDone(CardNo, axis);
        }

        /// <summary>
        /// 使能EtherCAT总线驱动器
        /// </summary>
        /// <param name="cardNo">卡号</param>
        /// <param name="axis">轴号(255表示使能所有轴)</param>
        /// <returns>错误码</returns>
        public static short SetAxisEnable( ushort axis )
        {
            return _leisai.SetAxisEnable(CardNo, axis);
        }

        /// <summary>
        /// 解除使能
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static short SetAxisDisable( ushort axis )
        {
            return _leisai.SetAxisDisable(CardNo, axis);
        }

        /// <summary>
        /// 设置轴的参数
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="minVel"></param>
        /// <param name="maxVel"></param>
        /// <param name="tacc"></param>
        /// <param name="tdec"></param>
        /// <param name="stopVel"></param>
        /// <returns></returns>
        public static void SetProfileUnit( ushort axis, double minVel, double maxVel )
        {
            _leisai.SetProfileUnit(CardNo, axis, minVel, maxVel, Axis_AccDec, Axis_AccDec, maxVel);
        }

        /// <summary>
        /// 启动轴的相对运动（定长运动）
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="dist"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static async Task Leisai_Pmov( ushort axis, double dist, ushort mode )
        {
            await _leisai.PmoveUnit(CardNo, axis, dist, mode);
        }

        /// <summary>
        /// Y轴移动至安全点位
        /// </summary>
        /// <returns></returns>
        public static async Task Leisai_Axis_Y_SafetyPoint_Pmov()
        {
            var axisYMovSafetyPoint = _leisai.PmoveUnit(CardNo, (ushort) AxisName.Y, AxisY_Safety_Pos, 1);
            var axisYINPDone = Axis_INP.AxisYINPAsync(AxisY_Safety_Pos);
            await Task.WhenAll(axisYMovSafetyPoint, axisYINPDone);
        }

        /// <summary>
        /// 启动轴的连续运动（定速运动）
        /// </summary>
        /// <param name="cardNo">卡号</param>
        /// <param name="axis">轴号</param>
        /// <param name="direction">运动方向(0-负方向, 1-正方向)</param>
        public static void Leisai_Vmov( ushort axis, ushort direction )
        {
            _leisai.Vmove(CardNo, axis, direction);
        }

        /// <summary>
        /// 停止指定轴的运动
        /// </summary>
        /// <param name="cardNo">卡号</param>
        /// <param name="axis">轴号(0表示所有轴)</param>
        /// <param name="stopMode">停止模式(0-减速停止,1-立即停止)</param>
        /// <returns>错误码</returns>
        public static void Leisai_Stop( ushort axis, ushort stopMode )
        {
            _leisai.Stop(CardNo, axis, stopMode);
        }

        /// <summary>
        /// 轴回原点
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static async Task<bool> Leisai_Home( ushort axis )
        {
            ushort homeMode;
            double lowVel;
            double highVel;
            bool isFlag = false;
            if ( axis == 0 || axis == 1 )
            {
                homeMode = 12;
                lowVel = AxisX_Y_Home_LowSpeed;
                highVel = AxisX_Y_Home_HighSpeed;
            }
            else
            {
                homeMode = 13;
                lowVel = AxisZ_Home_LowSpeed;
                highVel = AxisZ_Home_HighSpeed;
            }
            await _leisai.HomeMove(CardNo, axis, homeMode, lowVel, highVel, 0.5, 0.5);
            _leisai.GetHomeResult(CardNo, axis, out isFlag);
            return isFlag;
        }
    }
}