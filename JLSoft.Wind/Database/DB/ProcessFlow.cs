using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;

namespace JLSoft.Wind.Database.DB
{
    [DbTable("process_flow")]
    public class ProcessFlow
    {
        /// <summary>
        /// 主键，唯一标识每个流程
        /// </summary>
        [DbColumn("process_flow_id", IsPrimaryKey = true)]
        public Guid ProcessFlowId { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 生失效标识 0，1
        /// </summary>
        [DbColumn]
        public int Type { get; set; } = 0;
        /// <summary>
        /// 版本号
        /// </summary>
        [DbColumn]
        public int Versions { get; set; } = 1;
        /// <summary>
        /// 创建人
        /// </summary>
        [DbColumn]
        public string CreateName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [DbColumn]
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 生效人
        /// </summary>
        [DbColumn]
        public string ComeName { get; set; }
        /// <summary>
        /// 生效时间
        /// </summary>
        [DbColumn]
        public DateTime? ComeTime { get; set; }
        /// <summary>
        /// 失效人
        /// </summary>
        [DbColumn]
        public string FailureName { get; set; }
        /// <summary>
        /// 失效时间
        /// </summary>
        [DbColumn]
        public DateTime? FailureTime { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        [DbColumn]
        public string ProductName { get; set; }
        /// <summary>
        /// 产品工艺名称
        /// </summary>
        [DbColumn]
        public string ProductProcessName { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        [DbColumn]
        public string Spec { get; set; }

        /// <summary>
        /// 时间搓
        /// </summary>
        [DbColumn("last_update")]
        public DateTime LastUpdate 
        {
            get => _lastUpdate;
            set => _lastUpdate = value;
        }
        private DateTime _lastUpdate = DateTime.Now;
        public void UpdateTimestamp() => _lastUpdate = DateTime.Now;

    }
}
