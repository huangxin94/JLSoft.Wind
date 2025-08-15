using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Struct;
using SqlSugar;

namespace JLSoft.Wind.Database.DB
{
    [DbTable("users")]
    public class User
    {
        [DbColumn("userid", IsPrimaryKey = true)]
        public int UserId { get; set; }

        [DbColumn]
        public string Username { get; set; }

        [DbColumn]
        public string Password { get; set; }

        [DbColumn]
        public string Grade { get; set; }

        [DbColumn("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
