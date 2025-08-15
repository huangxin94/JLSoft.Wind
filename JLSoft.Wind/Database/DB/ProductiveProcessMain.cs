using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;

namespace JLSoft.Wind.Database.DB
{
    public class ProductiveProcessMain
    {
        [DbTable("ProductiveProcessMain")]
        public class DeviceStatus
        {
            /// <summary>
            /// 主键
            /// </summary>
            [DbColumn("MainId", IsPrimaryKey = true)]
            public Guid MainId { get; set; }
            /// <summary>
            /// 状态（生产、完成、异常）
            /// </summary>
            [DbColumn]
            public string Status { get; set; }
            /// <summary>
            /// 当前站点
            /// </summary>
            [DbColumn("CurrentSite")]
            public string CurrentSite { get; set; }
            /// <summary>
            /// 完成进度（百分比）
            /// </summary>
            [DbColumn("Progress")]
            public string Progress { get; set; }
            /// <summary>
            /// 工艺流程项ID，关联到ProcessFlowItem
            /// </summary>
            [DbColumn("ProcessFlowId")]
            public string ProcessFlowId { get; set; }
            /// <summary>
            /// 实际工艺流程，当前的实际工艺流程项ID
            /// </summary>
            [DbColumn("ActualProcessItemID")]
            public Guid? ActualProcessItemID { get; set; }
            /// <summary>
            /// 类型（Wafer、Glass、Assembled）
            /// </summary>
            [DbColumn("Type")]
            public string Type { get; set; }
            /// <summary>
            /// 产品名称
            /// </summary>
            [DbColumn("ProductName")]
            public string ProductName { get; set; }
            /// <summary>
            /// 产品编码
            /// </summary>
            [DbColumn("ProductCode")]
            public string ProductCode { get; set; }
            /// <summary>
            /// Glass编码
            /// </summary>
            [DbColumn("GlassId")]
            public string GlassId { get; set; }
            /// <summary>
            /// Wafer编码
            /// </summary>
            [DbColumn("WaferId")]
            public string WaferId { get; set; }
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



        }
    }
}
