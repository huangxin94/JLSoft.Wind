using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Class.Models
{
    public class JobDataRecord
    {
        public string JOB_ID { get; set; }
        public string JOBID_Pair { get; set; }
        public int VersionNumber { get; set; }
        public int Type { get; set; }
        public int Style { get; set; }
        public JobProcessFlag ProcessFlag { get; set; }
        public int PPID { get; set; }

        /// <summary>
        /// 当前工位的Job Data（全局唯一，进一出一）
        /// </summary>
        public static JobDataRecord? CurrentJobData { get; set; }

        public bool IsValid()
        {
            // 验证数据是否符合规范
            return VersionNumber > 0 && VersionNumber <= 2 &&
                   Type >= 1 && Type <= 3 &&
                   Style >= 1 && Style <= 4 &&
                   PPID >= 1 && PPID <= 999;
        }
    }

    public class JobProcessFlag
    {
        public bool V_Processed { get; set; }
        public bool S_Processed { get; set; }
        public bool G_Processed { get; set; }
        public bool U_Processed { get; set; }
        public bool A_Processed { get; set; }
        public bool C_Processed { get; set; }
        public bool R_Processed { get; set; }
        public bool M_Processed { get; set; }
    }
}
