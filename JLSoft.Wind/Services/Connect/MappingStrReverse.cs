using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Services.Connect
{
    public class MappingStrReverse
    {
        /// <summary>
        /// LoadPort扫片结果字符串反转（Robot不反转）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MapStrReverse(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            string reversed = new string(charArray);
            Console.WriteLine("反转后字符串: " + reversed);
            return reversed;
        }
    }
}
