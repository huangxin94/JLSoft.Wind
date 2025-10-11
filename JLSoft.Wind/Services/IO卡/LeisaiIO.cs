using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Logs;
using csIOC0640;

namespace JLSoft.Wind.Services
{
    public class LeisaiIO
    {

        public IOC0640_OPERATE_DATA m_data = new IOC0640_OPERATE_DATA();
        private IOC0640_OPERATE m_IniCallBack;//委托声明

        public static bool _EMO;
        public static bool P1_Wafer_8;
        public static bool P1_Wafer_12;
        public static bool P1_Door, X1_Door, Aligner_Door;
        private static bool _robot_Ready;
        private static bool _robot_Run;
        private static bool _robot_Fault;
        public static bool Robot_R_Vac;
        public static bool P1_Red_Btn;
        public static bool P1_Yellow_Btn;
        private static bool _p1_Green_Btn;
        public static bool X1_Wafer_8;
        public static bool X1_Wafer_12;
        public static bool X1_Door_1;
        public static bool _P1_EMO;
        public static bool _X1_EMO;
        public static bool _X1_Red_Btn;
        public static bool _X1_Green_Btn;

        public static bool CST_Temp1_8_1, CST_Temp1_8_2, CST_Temp1_12_1, CST_Temp1_12_2;
        public static bool CST_Temp2_8_1, CST_Temp2_8_2, CST_Temp2_12_1, CST_Temp2_12_2;
        public static bool CST_Temp3_8_1, CST_Temp3_8_2, CST_Temp3_12_1, CST_Temp3_12_2;
        public static bool _Axis_X_Seftey_S;
        /// <summary>
        /// X轴安全门光电  监测异物
        /// </summary>
        public static bool Axis_X_Seftey_S
        {
            get
            { return _Axis_X_Seftey_S; }
            set
            {
                if ( _Axis_X_Seftey_S != value )
                {
                    if ( true == value )
                    {
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.X, 1);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Y, 1);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Z, 1);
                        _ = HWRobot.Robot_StopAsync();
                    }
                }
                _Axis_X_Seftey_S = value;
            }
        }
        /// <summary>
        /// 控制台 急停
        /// </summary>
        public static bool EMO
        {
            get
            { return _EMO; }
            set
            {
                if ( _EMO != value )
                {
                    if ( true == value )
                    {
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.X, 0);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Y, 0);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Z, 0);
                        _ = HWRobot.Robot_StopAsync();
                    }
                }
                _EMO = value;
            }
        }
        /// <summary>
        /// P1 急停
        /// </summary>
        public static bool P1_EMO
        {
            get
            { return _P1_EMO; }
            set
            {
                if ( _P1_EMO != value )
                {
                    if ( true == value )
                    {
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.X, 0);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Y, 0);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Z, 0);
                        _ = HWRobot.Robot_StopAsync();
                    }
                }
                _P1_EMO = value;
            }
        }
        /// <summary>
        /// X1 急停
        /// </summary>
        public static bool X1_EMO
        {
            get
            { return _X1_EMO; }
            set
            {
                if ( _X1_EMO != value )
                {
                    if ( true == value )
                    {
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.X, 0);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Y, 0);
                        Leisai_Axis.Leisai_Stop((ushort) Leisai_Axis.AxisName.Z, 0);
                        _ = HWRobot.Robot_StopAsync();
                    }
                }
                _X1_EMO = value;
            }
        }
        /// <summary>
        /// Robot 故障
        /// </summary>
        public static bool Robot_Fault
        {
            get
            {
                return _robot_Fault;
            }
            set
            {
                if ( _robot_Fault != value )
                {
                    if ( value == true )
                    {
                        //Form1._instance.ROBOT_UI.BackColor = Color.Red;
                    }
                }
                _robot_Fault = value;
            }
        }
        /// <summary>
        /// Robot 正在动作
        /// </summary>
        public static bool Robot_Run
        {
            get
            {
                return _robot_Run;
            }
            set
            {
                if ( _robot_Run != value )
                {
                    if ( value == true )
                    {
                        //Form1._instance.ROBOT_UI.BackColor = Color.Green;
                    }
                }
                _robot_Run = value;
            }
        }
        /// <summary>
        /// Robot 等待可执行状态
        /// </summary>
        public static bool Robot_Ready
        {
            get
            {
                return _robot_Ready;
            }
            set
            {
                if ( _robot_Ready != value )
                {
                    if ( value == true )
                    {
                        ///Form1._instance.ROBOT_UI.BackColor = Color.Yellow;
                    }
                }
                _robot_Ready = value;
            }
        }
        /// <summary>
        /// P1 绿色按钮
        /// </summary>
        public static bool P1_Green_Btn
        {
            get
            {
                return _p1_Green_Btn;
            }
            set
            {
                if ( _p1_Green_Btn != value )
                {
                    if ( value == true )
                    {
                    }
                }
                _p1_Green_Btn = value;
            }
        }
        /// <summary>
        /// X1 绿色按钮
        /// </summary>
        public static bool X1_Green_Btn
        {
            get
            {
                return _X1_Green_Btn;
            }
            set
            {
                if ( _X1_Green_Btn != value )
                {
                    
                }
                _X1_Green_Btn = value;
            }
        }
        /// <summary>
        /// X1 红色按钮
        /// </summary>
        public static bool X1_Red_Btn
        {
            get
            {
                return _X1_Red_Btn;
            }
            set
            {
                if ( _X1_Red_Btn != value )
                {
                    
                }
                _X1_Red_Btn = value;
            }
        }

        /// <summary>
        /// 初始化IO卡
        /// </summary>
        /// <returns></returns>
        public static bool leisaiIO_Init()
        {
            try
            {
                int nCard = IOC0640.ioc_board_init();
                if (nCard <= 0)//控制卡初始化
                {
                    LogManager.Log($"[{DateTime.Now.ToString("HH:mm:ss")}] <Host> [Error] 未找到IOC0640控制卡!",Sunny.UI.LogLevel.Warn);
                    return false;
                }
                else
                {                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"[{DateTime.Now.ToString("HH:mm:ss")}] <Host> [Error] IOC0640控制卡初始化异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 关闭IO卡
        /// </summary>
        public static void leisaiIO_Close()
        {
            IOC0640.ioc_board_close();
        }
        /// <summary>
        /// 输入点状态刷新
        /// </summary>
        public static void ReadInputState()
        {
            //Control container = Form1._instance.Input_Pb;
            ushort bitno = 0;
            for ( int i = 1; i <= 32; i++ )
            {
                try
                {
                    bitno = (ushort) ( i );

                    var state = IOC0640.ioc_read_inbit(0, bitno);
                    //string buttonName = "Input" + i;
                    //Button btn = container.Controls[buttonName] as Button;
                    // 更新按钮颜色
                    
                    switch ( i )
                    {
                        case 1:
                            EMO = state == 1 ? true : false;
                            break;

                        case 2:
                            Robot_Ready = state == 0 ? true : false;
                            break;

                        case 3:
                            Robot_Run = state == 0 ? true : false;
                            break;

                        case 4:
                            Robot_Fault = state == 0 ? true : false;
                            break;

                        case 5:
                            Robot_R_Vac = state == 0 ? true : false;
                            break;

                        case 6:
                            P1_Wafer_8 = state == 0 ? true : false;
                            break;

                        case 7:
                            P1_Wafer_12 = state == 0 ? true : false;
                            break;

                        case 8:
                            P1_EMO = state == 1 ? true : false;
                            break;

                        case 9:
                            P1_Red_Btn = state == 0 ? true : false;
                            break;

                        case 10:
                            P1_Yellow_Btn = state == 0 ? true : false;
                            break;

                        case 11:
                            P1_Green_Btn = state == 0 ? true : false;
                            break;

                        case 12:
                            P1_Door = state == 0 ? true : false;
                            break;

                        case 13:
                            X1_Door = state == 0 ? true : false;
                            break;

                        case 14:
                            Aligner_Door = state == 0 ? true : false;
                            break;

                        case 15:
                            X1_Wafer_8 = state == 0 ? true : false;
                            break;

                        case 16:
                            X1_Wafer_12 = state == 0 ? true : false;
                            break;

                        case 17:
                            X1_EMO = state == 1 ? true : false;
                            //Form1._instance.X1_EMO_Btn.BackColor = X1_EMO ? Color.Red : Color.White;
                            break;

                        case 18:
                            X1_Red_Btn = state == 1 ? true : false;
                            break;

                        case 19:
                            X1_Green_Btn = state == 1 ? true : false;
                            break;

                        case 20:
                            CST_Temp1_8_1 = state == 1 ? true : false;
                            break;

                        case 21:
                            CST_Temp1_8_2 = state == 1 ? true : false;
                            break;

                        case 22:
                            CST_Temp1_12_1 = state == 1 ? true : false;
                            break;

                        case 23:
                            CST_Temp1_12_2 = state == 1 ? true : false;
                            break;

                        case 24:
                            CST_Temp2_8_1 = state == 1 ? true : false;
                            break;

                        case 25:
                            CST_Temp2_8_2 = state == 1 ? true : false;
                            break;

                        case 26:
                            CST_Temp2_12_1 = state == 1 ? true : false;
                            break;

                        case 27:
                            CST_Temp2_12_2 = state == 1 ? true : false;
                            break;

                        case 28:
                            CST_Temp3_8_1 = state == 1 ? true : false;
                            break;

                        case 29:
                            CST_Temp3_8_2 = state == 1 ? true : false;
                            break;

                        case 30:
                            CST_Temp3_12_1 = state == 1 ? true : false;
                            break;

                        case 31:
                            CST_Temp3_12_2 = state == 1 ? true : false;
                            break;

                        case 32:
                            Axis_X_Seftey_S = state == 1 ? true : false;
                            break;

                        default:
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine($"刷新输入点 {i} 异常: {ex.Message}");
                }
            }
        }

        public static void ReadOutputState()
        {
            ushort bitno = 0;
            for ( int i = 1; i <= 26; i++ )
            {
                try
                {
                    bitno = (ushort) ( i );

                    var state = IOC0640.ioc_read_outbit(0, bitno);

                }
                catch ( Exception ex )
                {
                    Console.WriteLine($"刷新输入点 {i} 异常: {ex.Message}");
                }
            }
        }

        public static void WriteOutput( ushort bitno )
        {
            var state = IOC0640.ioc_read_outbit(0, bitno);
            if ( state == 0 )
            {
                var status = IOC0640.ioc_write_outbit(0, bitno, 1);
            }
            else
            {
                var status = IOC0640.ioc_write_outbit(0, bitno, 0);
            }
        }
    }
}