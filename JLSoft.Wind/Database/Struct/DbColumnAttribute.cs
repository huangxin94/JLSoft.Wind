using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Database.Struct
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public bool IsPrimaryKey { get; set; }
        public bool IsNullable { get; set; } = true;
        public DbColumnAttribute(string columnName = null) => ColumnName = columnName;

    }
}
