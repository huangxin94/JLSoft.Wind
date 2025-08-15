using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using JLSoft.Wind.Database.Struct;

namespace JLSoft.Wind.Database.DB
{
    [DbTable("process_flow_item")]
    public class ProcessFlowItem
    {
        /// <summary>
        /// 主键，唯一标识每个流程
        /// </summary>
        [DbColumn(IsPrimaryKey = true)]
        public Guid ProcessFlowItemId { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 父表ID，关联到流程明细
        /// </summary>
        [DbColumn]
        public Guid ProcessFlowId { get; set; }
        /// <summary>
        /// 类型（Wafer、Glass、Assembled）
        /// </summary>
        [DbColumn]
        public string Type { get; set; }
        /// <summary>
        /// 工序名称
        /// </summary>
        [DbColumn]
        public string WorkstageName { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        [DbColumn]
        public int Seq { get; set; }
        /// <summary>
        /// 工序单步等待时间（单位：秒）
        /// </summary>
        [DbColumn]
        public int Qtime { get; set; }

    }
}
