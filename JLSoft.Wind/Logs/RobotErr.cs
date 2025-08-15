using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DuckDB.NET.Native.NativeMethods;

namespace JLSoft.Wind.Logs
{
    public class RobotErr
    {
        public static string msg_Log;

        public static string Msg_Log_UI
        {
            get { return msg_Log; }
            set
            {
                //Form1.Log_Info_UI(value);
                //Log_Info(value);  //记录日志弹出窗口及执行错误事件

                Logger.Instance.WriteLine(value);
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="RobotCommException"></exception>
        private static void Log_Info(string value)
        {
            // 接收到错误代码后执行事件....

            //throw new RobotCommException($"{value}");
        }

        #region //解析错误码

        /// <summary>
        /// 解析错误码
        /// </summary>
        /// <param name="str"></param>
        public static void Robot_Err_switch(string str)
        {
            string[] nametk = str.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < 8; i++)
            {
                int b16 = Convert.ToInt32(nametk[i], 16);
                string binary = Convert.ToString(b16, 2);
                string b2 = binary.PadLeft(16, '0');
                int add = 0;

                for (int j = 16; j > 0; j--)
                {
                    int err = 0;
                    add++;
                    string resultString = b2.Substring(j - 1, 1);
                    if (resultString == "1")
                    {
                        err = add;
                        Console.WriteLine(add);
                    }

                    switch (i)
                    {
                        case 0:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 101:指令输入错误！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 102:站名输入错误！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 103:轴名输入错误！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 104:群组名称输入错误！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 105:引数输入错误！";
                                    break;

                                case 6:
                                    Msg_Log_UI = "<Robot> Error 106:输入距离超过软体极限！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 107:目前处于PAUSE状态无法下达指令！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 108:站点I/O互锁错误，输入信号与设定不符或输入信号中断！";
                                    break;
                            }
                            break;

                        case 1:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 201:取放片前，真空状态已被启动！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 202:取放片后，真空状态无法被产生或解除！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 203:Z轴硬体已触发上极限！";

                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 204:Z轴硬体已触发下极限！";

                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 205:取放片前，光纤感测器状态已被启动！";
                                    break;

                                case 6:
                                    Msg_Log_UI = "<Robot> Error 206:取放片前，光纤感测器状态已被解除！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 207:磁簧开关为伸出状态，无法被缩回！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 208:磁簧开关为伸出状态，无法被伸出！";
                                    break;

                                case 9:
                                    Msg_Log_UI = "<Robot> Error 209:H轴硬体已触发正极限！";

                                    break;

                                case 10:
                                    Msg_Log_UI = "<Robot> Error 210:H轴硬体已触发负极限！";

                                    break;

                                case 11:
                                    Msg_Log_UI = "<Robot> Error 211:翻转气缸未翻至正面！";
                                    break;

                                case 12:
                                    Msg_Log_UI = "<Robot> Error 212:翻转气缸未翻至反面！";
                                    break;

                                case 13:
                                    Msg_Log_UI = "<Robot> Error 213:R轴在执行取放片指令时，晶圆掉落！";
                                    break;

                                case 14:
                                    Msg_Log_UI = "<Robot> Error 214:W轴在执行取放片指令时，晶圆掉落！";
                                    break;

                                case 15:
                                    Msg_Log_UI = "<Robot> Error 215:取晶圆前，正压状态已被启动！";
                                    break;

                                case 16:
                                    Msg_Log_UI = "<Robot> Error 216:取放片后，正压状态无法产生或解除！";
                                    break;
                            }
                            break;

                        case 2:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 301:马达未激磁或激磁失败！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 302:马达未回原点或回原点失败！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 303:马达正在移动！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 304:跟随误差超过设定的最大位置误差值！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 305:伺服马达编码器错误！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 307:伺服马达温度过高！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 308:马达移动至行程软体正极限位置！";
                                    break;

                                case 9:
                                    Msg_Log_UI = "<Robot> Error 309:马达移动至行程软体负极限位置！";
                                    break;

                                case 10:
                                    Msg_Log_UI = "<Robot> Error 310:马达任一轴速度或加减速参数异常！";
                                    break;

                                case 11:
                                    Msg_Log_UI = "<Robot> Error 311:T/Z/H轴回原点时，R/W轴尚未回原点！";
                                    break;

                                case 12:
                                    Msg_Log_UI = "<Robot> Error 312:T/Z/H轴回原点时，R/W轴未缩回到安全位置！";
                                    break;

                                case 13:
                                    // Msg_Log_UI = "<Robot> Error 313:驱动板温度过高！";
                                    break;
                            }
                            break;

                        case 3:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 301:马达未激磁或激磁失败！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 302:马达未回原点或回原点失败！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 303:马达正在移动！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 304:跟随误差超过设定的最大位置误差值！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 305:伺服马达编码器错误！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 307:伺服马达温度过高！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 308:马达移动至行程软体正极限位置！";
                                    break;

                                case 9:
                                    Msg_Log_UI = "<Robot> Error 309:马达移动至行程软体负极限位置！";
                                    break;

                                case 10:
                                    Msg_Log_UI = "<Robot> Error 310:马达任一轴速度或加减速参数异常！";
                                    break;

                                case 11:
                                    Msg_Log_UI = "<Robot> Error 311:T/Z/H轴回原点时，R/W轴尚未回原点！";
                                    break;

                                case 12:
                                    Msg_Log_UI = "<Robot> Error 312:T/Z/H轴回原点时，R/W轴未缩回到安全位置！";
                                    break;

                                case 13:
                                    // Msg_Log_UI = "<Robot> Error 313:驱动板温度过高！";
                                    break;
                            }
                            break;

                        case 4:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 301:马达未激磁或激磁失败！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 302:马达未回原点或回原点失败！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 303:马达正在移动！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 304:跟随误差超过设定的最大位置误差值！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 305:伺服马达编码器错误！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 307:伺服马达温度过高！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 308:马达移动至行程软体正极限位置！";
                                    break;

                                case 9:
                                    Msg_Log_UI = "<Robot> Error 309:马达移动至行程软体负极限位置！";
                                    break;

                                case 10:
                                    Msg_Log_UI = "<Robot> Error 310:马达任一轴速度或加减速参数异常！";
                                    break;

                                case 11:
                                    Msg_Log_UI = "<Robot> Error 311:T/Z/H轴回原点时，R/W轴尚未回原点！";
                                    break;

                                case 12:
                                    Msg_Log_UI = "<Robot> Error 312:T/Z/H轴回原点时，R/W轴未缩回到安全位置！";
                                    break;

                                case 13:
                                    // Msg_Log_UI = "<Robot> Error 313:驱动板温度过高！";
                                    break;
                            }
                            break;

                        case 5:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 301:马达未激磁或激磁失败！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 302:马达未回原点或回原点失败！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 303:马达正在移动！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 304:跟随误差超过设定的最大位置误差值！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 305:伺服马达编码器错误！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 307:伺服马达温度过高！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 308:马达移动至行程软体正极限位置！";
                                    break;

                                case 9:
                                    Msg_Log_UI = "<Robot> Error 309:马达移动至行程软体负极限位置！";
                                    break;

                                case 10:
                                    Msg_Log_UI = "<Robot> Error 310:马达任一轴速度或加减速参数异常！";
                                    break;

                                case 11:
                                    Msg_Log_UI = "<Robot> Error 311:T/Z/H轴回原点时，R/W轴尚未回原点！";
                                    break;

                                case 12:
                                    Msg_Log_UI = "<Robot> Error 312:T/Z/H轴回原点时，R/W轴未缩回到安全位置！";
                                    break;

                                case 13:
                                    // Msg_Log_UI = "<Robot> Error 313:驱动板温度过高！";
                                    break;
                            }
                            break;

                        case 6:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 301:马达未激磁或激磁失败！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 302:马达未回原点或回原点失败！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 303:马达正在移动！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 304:跟随误差超过设定的最大位置误差值！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 305:伺服马达编码器错误！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 307:伺服马达温度过高！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 308:马达移动至行程软体正极限位置！";
                                    break;

                                case 9:
                                    Msg_Log_UI = "<Robot> Error 309:马达移动至行程软体负极限位置！";
                                    break;

                                case 10:
                                    Msg_Log_UI = "<Robot> Error 310:马达任一轴速度或加减速参数异常！";
                                    break;

                                case 11:
                                    Msg_Log_UI = "<Robot> Error 311:T/Z/H轴回原点时，R/W轴尚未回原点！";
                                    break;

                                case 12:
                                    Msg_Log_UI = "<Robot> Error 312:T/Z/H轴回原点时，R/W轴未缩回到安全位置！";
                                    break;

                                case 13:
                                    // Msg_Log_UI = "<Robot> Error 313:驱动板温度过高！";
                                    break;
                            }
                            break;

                        case 7:
                            switch (err)
                            {
                                case 1:
                                    Msg_Log_UI = "<Robot> Error 401:急停已触发！";
                                    break;

                                case 2:
                                    Msg_Log_UI = "<Robot> Error 402:控制器电源故障！";
                                    break;

                                case 3:
                                    Msg_Log_UI = "<Robot> Error 403:控制器电压过低！";
                                    break;

                                case 4:
                                    Msg_Log_UI = "<Robot> Error 404:控制器电压过高！";
                                    break;

                                case 5:
                                    Msg_Log_UI = "<Robot> Error 405:控制器判定驱动器故障！";
                                    break;

                                case 6:
                                    Msg_Log_UI = "<Robot> Error 406:控制器电压异常！";
                                    break;

                                case 7:
                                    Msg_Log_UI = "<Robot> Error 407:控制器无法识别驱动器！";
                                    break;

                                case 8:
                                    Msg_Log_UI = "<Robot> Error 408:控制器UPS故障！";
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        #endregion //解析错误码
    }
}
