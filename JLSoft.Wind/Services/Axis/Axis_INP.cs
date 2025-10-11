using System;
using System.Threading;
using System.Threading.Tasks;

public class Axis_INP
{
    public static bool Axis_X_INP;
    public static bool Axis_Y_INP;
    public static bool Axis_Z_INP;
    public static double X_pos;
    public static double Y_pos;
    public static double Z_pos;

    /// <summary>
    /// 判断当前是否在站点位置
    /// </summary>
    /// <param name="setPoint"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static bool AxisPosNowAsync(double setPointX, double setPointY, double setPointZ)
    {

        double posNowX = Math.Round(Axis_INP.X_pos, 3);
        double posNowY = Math.Round(Axis_INP.Y_pos, 3);
        double posNowZ = Math.Round(Axis_INP.Z_pos, 3);
        if (posNowX == setPointX && posNowY == setPointY && posNowZ == setPointZ)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 判断X轴是否已到位
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="setPoint"></param>
    /// <returns></returns>
    public static async Task<bool> AxisXINPAsync( double setPoint, int timeout = 600000 )
    {
        double posNow = 0;
        bool monitor = false;
        var cancellationTokenSource = new CancellationTokenSource(timeout);

        try
        {
            while ( !monitor && !cancellationTokenSource.Token.IsCancellationRequested )
            {

                    posNow = Math.Round( Axis_INP.X_pos,3);
                    if ( setPoint != posNow )
                    {
                        Axis_X_INP = false;
                        await Task.Delay(20, cancellationTokenSource.Token);
                    }
                    else
                    {
                        Axis_X_INP = true;
                        monitor = true;
                        break;
                    }
                
            }
        }
        catch ( OperationCanceledException )
        {
            Console.WriteLine("轴到位判断超时");
        }

        return monitor;
    }

    /// <summary>
    /// 判断轴是否已到位
    /// </summary>
    /// <param name="setPoint"></param>
    /// <returns></returns>
    public static async Task<bool> AxisYINPAsync( double setPoint, int timeout = 600000 )
    {
        double posNow = 0;
        bool monitor = false;
        var cancellationTokenSource = new CancellationTokenSource(timeout);

        try
        {
            while ( !monitor && !cancellationTokenSource.Token.IsCancellationRequested )
            {

                posNow = Math.Round(Axis_INP.Y_pos, 3);
                if ( setPoint != posNow )
                {
                    Axis_Y_INP = false;
                    await Task.Delay(20, cancellationTokenSource.Token);
                }
                else
                {
                    Axis_Y_INP = true;
                    monitor = true;
                    break;
                }

            }
        }
        catch ( OperationCanceledException )
        {
            Console.WriteLine("轴到位判断超时");
        }

        return monitor;
    }

    /// <summary>
    /// 判断轴是否已到位
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="setPoint"></param>
    /// <returns></returns>
    public static async Task<bool> AxisZINPAsync( double setPoint, int timeout = 600000 )
    {
        double posNow = 0;
        bool monitor = false;
        var cancellationTokenSource = new CancellationTokenSource(timeout);

        try
        {
            while ( !monitor && !cancellationTokenSource.Token.IsCancellationRequested )
            {

                posNow = Math.Round(Axis_INP.Z_pos, 3);
                if ( setPoint != posNow)
                {
                    Axis_Z_INP = false;
                    await Task.Delay(20, cancellationTokenSource.Token);
                }
                else
                {
                    Axis_Z_INP = true;
                    monitor = true;
                    break;
                }

            }
        }
        catch ( OperationCanceledException )
        {
            Console.WriteLine("轴到位判断超时");
        }

        return monitor;
    }
}