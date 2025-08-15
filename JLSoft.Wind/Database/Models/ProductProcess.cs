using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 工艺流程模型
    /// </summary>
    public class ProductProcess
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 工艺名称
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 是否生效
        /// </summary>
        public bool IsActive { get; set; }    
    }
}
