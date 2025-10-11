using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Database;

namespace JLSoft.Wind.Services
{
    public class LoadPortAndRobot_Wafer_UI
    {
        /// <summary>
        /// 读取RSR并显示UI状态
        /// </summary>
        /// <param name="rsr"></param>
        public static void Mapping_UI( StationName.Station station, string rsr )
        {
            if ( station == StationName.Station.LoadPort1 )
            {
                MainForm._instance.loadPort21.UpdateMapping(rsr);
            }
            else
            {
                MainForm._instance.loadPort22.UpdateMapping(rsr);
            }

        }

        /// <summary>
        /// GET/PUT后将CST内WAFER状态显示到UI
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="state"></param>
        public static void Cst_Wafer_UI( StationName.Station station, int slot, string state )
        {
            string cName;
            Control c;
            if ( station == StationName.Station.LoadPort1 )
            {
                if (state == "get")
                {
                    MainForm._instance.loadPort21.SetTextBoxColor(slot, Color.Gray);
                }
                else if (state == "put")
                {
                    MainForm._instance.loadPort21.SetTextBoxColor(slot, Color.Green);
                }
            }
            else if ( station == StationName.Station.LoadPort2 )
            {
                if (state == "get")
                {
                    MainForm._instance.loadPort22.SetTextBoxColor(slot, Color.Gray);
                }
                else if (state == "put")
                {
                    MainForm._instance.loadPort22.SetTextBoxColor(slot, Color.Green);
                }
            }
            else
            {
                if ( station == StationName.Station.P1 )
                {
                }
            }
        }
    }
}