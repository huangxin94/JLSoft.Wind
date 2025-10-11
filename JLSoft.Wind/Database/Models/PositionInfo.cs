using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Models
{
    public class PositionInfo
    {
        /// <summary>
        /// 实际上是设备唯一标识符
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string biename { get; set; }
        public Positions positions { get; set; }
    }
}
