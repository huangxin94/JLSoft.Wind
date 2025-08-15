using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DuckDB.NET.Data;
using JLSoft.Wind.Database;
using JLSoft.Wind.Database.Struct;
using NLog;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static DuckDB.NET.Native.NativeMethods;

namespace JLSoft.Wind.Services.DuckDb
{
    public static class DuckDbORM
    {
        private static readonly Dictionary<string, string> TableSchemas = new Dictionary<string, string>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void RegisterModels()
        {
            try
            {
                logger.Info("开始扫描ORM实体类...");

                // 获取所有程序集中带有DbTable特性的类
                var modelTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.GetCustomAttribute<DbTableAttribute>() != null)
                    .ToList();

                logger.Info($"找到 {modelTypes.Count} 个实体类");

                foreach (var type in modelTypes)
                {
                    logger.Debug($"处理实体类: {type.Name}");

                    var tableAttr = type.GetCustomAttribute<DbTableAttribute>();
                    var tableName = tableAttr?.TableName?.ToLower() ?? type.Name.ToLower();

                    var columns = new List<ColumnInfo>();

                    foreach (var prop in type.GetProperties())
                    {
                        var colAttr = prop.GetCustomAttribute<DbColumnAttribute>();
                        var isPrimary = colAttr?.IsPrimaryKey ?? false;
                        var isNullable = colAttr?.IsNullable ?? true;
                        var dbGenAttr = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();

                        // 获取属性初始化器的默认值
                        object defaultValue = null;
                        try
                        {
                            defaultValue = prop.GetValue(Activator.CreateInstance(type));
                        }
                        catch { }

                        columns.Add(new ColumnInfo
                        {
                            Name = colAttr?.ColumnName?.ToLower() ?? prop.Name.ToLower(),
                            Type = GetDuckDBType(prop.PropertyType),
                            IsPrimary = isPrimary,
                            IsNullable = isNullable,
                            DefaultValue = defaultValue,
                            PropertyType = prop.PropertyType
                        });
                    }

                    // 获取主键列列表
                    var primaryKeyColumns = columns.Where(c => c.IsPrimary).ToList();
                    bool isSinglePrimaryKey = primaryKeyColumns.Count == 1;

                    // 判断是否有自增主键列
                    var identityColumns = columns.Where(c => c.IsIdentity).ToList();
                    bool hasIdentityPrimary = identityColumns.Count == 1 &&
                                              identityColumns[0].IsPrimary &&
                                              isSinglePrimaryKey;

                    // 构建列定义
                    var columnDefs = new List<string>();

                    foreach (var col in columns)
                    {
                        string def = $"{col.Name} {col.Type}";

                        // 非自增主键处理
                        if (col.IsPrimary && isSinglePrimaryKey)
                            def += " PRIMARY KEY";

                        if (!col.IsNullable)
                            def += " NOT NULL";

                        // 自增列跳过默认值
                        if (col.DefaultValue != null && !col.IsIdentity)
                        {
                            var defaultValueStr = FormatDefaultValue(col.DefaultValue, col.PropertyType, col.Type);
                            if (!string.IsNullOrEmpty(defaultValueStr))
                            {
                                def += " " + defaultValueStr;
                            }
                        }

                        columnDefs.Add(def);
                    }

                    // 只有多主键时才添加表级主键
                    if (!isSinglePrimaryKey && primaryKeyColumns.Count > 0)
                    {
                        columnDefs.Add($"PRIMARY KEY ({string.Join(", ", primaryKeyColumns.Select(c => c.Name))})");
                    }

                    var createSql = $"CREATE TABLE IF NOT EXISTS {tableName} (" +
                                    string.Join(", ", columnDefs) + ")";

                    logger.Debug($"表 {tableName} 创建语句:\n{createSql}");
                    TableSchemas[tableName] = createSql;

                }

                logger.Info("实体类扫描完成");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "注册实体类时发生错误");
                throw;
            }
        }

        public static void MigrateDatabase(DuckDBConnection connection)
        {
            try
            {
                logger.Info("开始数据库迁移...");
                int tablesCreated = 0;
                int columnsAdded = 0;
                int sequencesCreated = 0;
                int defaultsSet = 0;

                // 第一步：创建或更新表结构
                foreach (var kv in TableSchemas.Where(kv => !kv.Key.StartsWith("seq_")))
                {
                    var tableName = kv.Key;
                    var createSql = kv.Value;

                    logger.Debug($"处理表 {tableName}");

                    // 检查表是否存在
                    bool tableExists = false;
                    using (var checkCmd = connection.CreateCommand())
                    {
                        checkCmd.CommandText = $@"
                            SELECT COUNT(*) 
                            FROM information_schema.tables 
                            WHERE table_name = '{tableName}'";

                        var result = checkCmd.ExecuteScalar();
                        tableExists = Convert.ToInt64(result) > 0;
                    }

                    if (!tableExists)
                    {
                        // 创建新表
                        using (var createCmd = connection.CreateCommand())
                        {
                            createCmd.CommandText = createSql;
                            createCmd.ExecuteNonQuery();
                            tablesCreated++;
                            logger.Info($"创建新表: {tableName}");
                        }
                    }
                    else
                    {
                        // 获取现有的列
                        var existingColumns = new List<string>();
                        using (var columnsCmd = connection.CreateCommand())
                        {
                            columnsCmd.CommandText = $@"
                                SELECT column_name 
                                FROM information_schema.columns 
                                WHERE table_name = '{tableName}'";

                            using (var reader = columnsCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existingColumns.Add(reader.GetString(0).ToLower());
                                }
                            }
                        }

                        // 解析模型中定义的列
                        var modelColumns = new List<string>();
                        // 提取列定义部分：CREATE TABLE ... (列定义)
                        var start = createSql.IndexOf('(') + 1;
                        var end = createSql.LastIndexOf(')');
                        var columnsStr = createSql.Substring(start, end - start);

                        var columnDefs = columnsStr.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !s.StartsWith("PRIMARY KEY"))
                            .ToList();

                        foreach (var colDef in columnDefs)
                        {
                            // 提取列名（第一个标识符）
                            var match = Regex.Match(colDef, @"^\w+");
                            if (match.Success)
                            {
                                var colName = match.Value.ToLower();
                                if (!existingColumns.Contains(colName))
                                {
                                    // 添加缺失的列
                                    using (var addColCmd = connection.CreateCommand())
                                    {
                                        // 去除列定义中的NOT NULL约束（如果表已有数据）
                                        var safeColDef = colDef.Replace(" NOT NULL", "");
                                        addColCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {safeColDef}";
                                        addColCmd.ExecuteNonQuery();
                                        columnsAdded++;
                                        logger.Info($"表 {tableName} 添加新列: {colName}");
                                    }
                                }
                            }
                        }
                    }
                }

                logger.Info($"数据库迁移完成: " +
                            $"创建了 {tablesCreated} 个表, " +
                            $"添加了 {columnsAdded} 个列, " +
                            $"创建了 {sequencesCreated} 个序列, " +
                            $"设置了 {defaultsSet} 个自增默认值");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "数据库迁移过程中发生错误");
                throw;
            }
        }

        public static string GetDuckDBType(Type type)
        {
            // 处理可空类型
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (type == typeof(int))
                return "INTEGER";
            if (type == typeof(double))
                return "DOUBLE";
            if (type == typeof(string))
                return "TEXT";
            if (type == typeof(DateTime))
                return "TIMESTAMP";
            if (type == typeof(bool))
                return "BOOLEAN";
            if (type == typeof(float))
                return "FLOAT";
            if (type == typeof(decimal))
                return "DECIMAL(18,6)";
            if (type == typeof(long))
                return "BIGINT";
            if (type == typeof(short))
                return "SMALLINT";
            if (type == typeof(byte[]))
                return "BLOB";
            if (type == typeof(Guid))
                return "UUID";
            return "TEXT";
        }

        private static string FormatDefaultValue(object defaultValue, Type propertyType, string columnType)
        {

            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                return ""; // 直接返回空字符串，不添加默认值
            }
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // 特殊处理可空类型
                var hasValue = (bool)propertyType.GetProperty("HasValue")?.GetValue(defaultValue);
                if (!hasValue) return "DEFAULT NULL";
            }

            // 处理 DateTime 类型的默认值（特别是当前时间）
            if (defaultValue is DateTime dt)
            {
                if (dt == DateTime.MinValue)
                    return "'1970-01-01 00:00:00'::TIMESTAMP";

                // 特殊处理当前时间
                if (dt == DateTime.Now || dt == DateTime.UtcNow)
                {
                    return "CURRENT_TIMESTAMP";
                }

                return $"'{dt:yyyy-MM-dd HH:mm:ss}'::TIMESTAMP";
            }
            // GUID 类型加单引号
            if (defaultValue is Guid guid)
                return $"DEFAULT '{guid}'";
            // 字符串类型加单引号
            if (defaultValue is string s)
            {
                return $"DEFAULT '{s.Replace("'", "''")}'"; // 转义单引号
            }

            // 布尔值转数字
            if (defaultValue is bool b)
            {
                return $"DEFAULT {(b ? "1" : "0")}";
            }

            // 空值特殊处理
            if (defaultValue == null)
            {
                return "DEFAULT NULL";
            }

            // 其他类型直接转换
            return $"DEFAULT {defaultValue}";
        }

        private class ColumnInfo
        {
            /// <summary>
            /// 字段名称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 字段类型
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// 是否主键
            /// </summary>
            public bool IsPrimary { get; set; }
            /// <summary>
            /// 是否允许空值
            /// </summary>
            public bool IsNullable { get; set; } = true;
            /// <summary>
            /// 是否自增
            /// </summary>
            public bool IsIdentity { get; set; }
            /// <summary>
            /// 默认值
            /// </summary>
            public object DefaultValue { get; set; }
            /// <summary>
            /// 属性类型
            /// </summary>
            public Type PropertyType { get; set; }
        }
    }
}