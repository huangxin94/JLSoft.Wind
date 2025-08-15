using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JLSoft.Wind.Database.Models
{
    /// <summary>
    /// 用户数据模型，表示系统中的用户信息。
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// 用户ID，唯一标识一个用户。
        /// </summary>
        [JsonProperty("UserId")]
        public int UserId { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>

        [JsonProperty("Username")]
        public string Username { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>

        [JsonProperty("Password")]
        public string Password { get; set; }
        /// <summary>
        /// 用户职阶    
        /// </summary>

        [JsonProperty("Grade")]
        public string Grade { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>

        [JsonProperty("LastUpdate")]
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
}
