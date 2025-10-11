using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Services
{
    public class WaferID_UI
    {
        public static bool Ocr_str_UI( string str )
        {
            List<string> list = str.Split(',').ToList();
            if ( list[7] == "OK" )
            {
                MainForm._instance.but_OCRStatue.BackColor = Color.Lime;
                MainForm._instance.but_OCRStatue.Text = "OK";
                MainForm._instance.txt_WaferID.Text = list[9].Replace(" ", "");
                return true;
            }
            else
            {
                MainForm._instance.but_OCRStatue.BackColor = Color.Red;
                MainForm._instance.but_OCRStatue.Text = "NG";
                MainForm._instance.txt_WaferID.Text = "";
                return false;
            }
        }
    }
}