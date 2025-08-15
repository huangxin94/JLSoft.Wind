using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;

namespace JLSoft.Wind.Database.DB
{
    /// <summary>
    /// 实际工艺流程项
    /// </summary>
    [DbTable("ActualProcessItem")]
    public class ActualProcessItem
    {
        /// <summary>
        /// 主键
        /// </summary>
        [DbColumn("ActualProcessItemID", IsPrimaryKey = true)]
        public Guid ActualProcessItemID { get; set; }
        /// <summary>
        /// 主表ID，关联到ProductiveProcessMain
        /// </summary>
        [DbColumn]
        public Guid PPMainId { get; set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        [DbColumn]
        public int Sequence { get; set; }
        /// <summary>
        /// 状态（未生产、生产中、已完成生产、异常完结）
        /// </summary>
        [DbColumn]
        public string Status { get; set; } = "0"; // 默认未生产状态
        /// <summary>
        /// 生失效标识（1-有效，0-失效）
        /// </summary>
        [DbColumn]
        public int Type { get; set; } = 1; // 默认有效状态
        /// <summary>
        /// 工序名称
        /// </summary>
        [DbColumn]
        public string WorkstageName { get; set; }
        /// <summary>
        /// 设定Qtime值
        /// </summary>
        [DbColumn]
        public int Qtime { get; set; } = 0; // 默认工序单步等待时间为0
        /// <summary>
        /// 开始时间
        /// </summary>
        [DbColumn("StartTime")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>

        [DbColumn("EndTime")]
        public DateTime EndTime { get; set; }
        /// <summary>
        ///  默认工序单步等待时间为0
        /// </summary>
        [DbColumn]
        public int ActualQtime { get; set; } = 0; // 默认工序单步等待时间为0
        /// <summary>
        /// 开始计算Qtime时间
        /// </summary>
        [DbColumn("StartQtime")]
        public DateTime StartQtime { get; set; }
        /// <summary>
        /// 结束计算Qtime时间
        /// </summary>

        [DbColumn("EndQtime")]
        public DateTime EndQtime { get; set; }

    }
}
