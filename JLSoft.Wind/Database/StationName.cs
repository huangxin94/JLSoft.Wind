using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database
{
    public class StationName
    {
        public enum Group
        {
            Wafer8,
            Wafer12,
            Square8,
            Square12
        }
        /// <summary>
        /// 机器人站名称
        /// </summary>
        public enum Station
        {
            LoadPort1,
            LoadPort2,
            Aligner,
            AngleT,
            P1,
            X1,
            S4,
            V1,
            M1,
            S3,
            U1_1,
            U1_2,
            C1,
            R1_1,
            R1_2,
            R1_3,
            R1_4,
            G1,
            A1,
            A2,
            A3,
            A4,
            TempCST_1_1,
            TempCST_1_2,
            TempCST_2_1,
            TempCST_2_2,
            TempCST_3_1,
            TempCST_3_2
        }
        /// <summary>
        /// 晶圆类型Group名转成字符
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string GetWaferType(Group group)
        {
            switch (group)
            {
                case Group.Wafer8: return "A";
                case Group.Wafer12: return "B";
                case Group.Square8: return "C";
                case Group.Square12: return "D";

                default: return group.ToString();
            }
        }
        /// <summary>
        /// 获取机器人站别名
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public static string GetAlias(Station station)
        {
            switch (station)
            {
                case Station.R1_1: return "AA";
                case Station.R1_2: return "AB";
                case Station.R1_3: return "AC";
                case Station.R1_4: return "AD";
                case Station.U1_1: return "BA";
                case Station.U1_2: return "BB";
                case Station.TempCST_1_1: return "CA";
                case Station.TempCST_1_2: return "CB";
                case Station.TempCST_2_1: return "DA";
                case Station.TempCST_2_2: return "DB";
                case Station.TempCST_3_1: return "EA";
                case Station.TempCST_3_2: return "EB";
                case Station.LoadPort1: return "F";
                case Station.LoadPort2: return "G";
                case Station.Aligner: return "H";
                case Station.AngleT: return "I";
                case Station.P1: return "J";
                case Station.X1: return "K";
                case Station.S4: return "L";
                case Station.V1: return "M";
                case Station.M1: return "N";
                case Station.S3: return "O";
                case Station.C1: return "P";
                case Station.G1: return "Q";
                case Station.A1: return "R";
                case Station.A2: return "S";
                case Station.A3: return "T";
                case Station.A4: return "U";

                default: return station.ToString();
            }
        }

        public static Station ChangeStaName(string station)
        {
            switch (station)
            {
                case "AA": return  Station.R1_1;
                case "AB": return  Station.R1_2;
                case "AC": return  Station.R1_3;
                case "AD": return  Station.R1_4;
                case "BA": return  Station.U1_1;
                case "BB": return Station.U1_2;
                case "CA": return Station.TempCST_1_1;
                case "CB": return Station.TempCST_1_2;
                case "DA": return Station.TempCST_2_1;
                case "DB": return Station.TempCST_2_2;
                case "EA": return Station.TempCST_3_1;
                case "EB": return Station.TempCST_3_2;
                case "F": return Station.LoadPort1;
                case "G": return Station.LoadPort2;
                case "H": return Station.Aligner;
                case "I": return Station.AngleT;
                case "J": return Station.P1;
                case "K": return Station.X1;
                case  "L": return Station.S4;
                case  "M": return Station.V1;
                case  "N": return Station.M1;
                case  "O": return Station.S3;
                case  "P": return Station.C1;
                case  "Q": return Station.G1;
                case  "R": return Station.A1;
                case  "S": return Station.A2;
                case  "T": return Station.A3;
                case "U": return Station.A4;

                default: return Station.A4;
            }
        }
    }
}
