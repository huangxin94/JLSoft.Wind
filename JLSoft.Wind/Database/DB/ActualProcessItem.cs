using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;

namespace JLSoft.Wind.Database.DB
{
    [DbTable("actualprocessitem")]
    public class ActualProcessItem
    {
        /// <summary>
        /// 主键
        /// </summary>
        [DbColumn("ActualProcessItemId", IsPrimaryKey = true)]
        public Guid ActualProcessItemId { get; set; }

        /// <summary>
        /// 关联到ProductiveProcessMain的主键
        /// </summary>
        [DbColumn("PPMainId")]
        [ForeignKey("ProductiveProcessMain")]
        public Guid PPMainId { get; set; }

        /// <summary>
        /// 步骤序号
        /// </summary>
        [DbColumn("Sequence")]
        public int Sequence { get; set; }

        /// <summary>
        /// 状态: 
        /// 0=未开始, 1=进行中, 2=完成, 3=异常
        /// </summary>
        [DbColumn("Status")]
        public string Status { get; set; } = "0"; // 默认未开始
        /// <summary>
        /// 是否删除
        /// </summary>
        [DbColumn("Type")]
        public int Type { get; set; } = 1; 


        /// <summary>
        /// 步骤名称
        /// </summary>
        [DbColumn("WorkstageName")]
        public string WorkstageName { get; set; }
        /// <summary>
        /// 计划Qtime
        /// </summary>
        [DbColumn("Qtime")]
        public double? Qtime { get; set; } = 0;

        /// <summary>
        /// 工序开始时间
        /// </summary>
        [DbColumn("StartTime")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 工序结束时间
        /// </summary>
        [DbColumn("EndTime")]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 实际Qtime
        /// </summary>
        [DbColumn("ActualQtime")]
        public double? ActualQtime { get; set; }
        /// <summary>
        /// 开始计算Qtime时间
        /// </summary>
        [DbColumn("StartQtime")]
        public DateTime? StartQtime { get; set; }

        /// <summary>
        /// 结束计算Qtime时间
        /// </summary>
        [DbColumn("EndQtime")]
        public DateTime? EndQtime { get; set; }
    }
}
