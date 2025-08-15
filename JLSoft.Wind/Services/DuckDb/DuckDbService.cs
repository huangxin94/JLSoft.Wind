using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using DuckDB.NET.Data;

namespace JLSoft.Wind.Services.DuckDb
{


    public class DuckDbService : IDisposable
    {
        private readonly string _connectionString;
        private static readonly ConcurrentDictionary<string, string> TableSchemas = new();
        private static readonly object _connectionLock = new();

        public DuckDbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void RegisterTableSchema(string tableName, string createSql)
        {
            var normalizedName = tableName.Trim().ToLower();
            TableSchemas[normalizedName] = createSql;
        }

        private DuckDBConnection GetConnection()
        {
            var conn = new DuckDBConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public DataTable ExecuteQuery(string query, params DuckDBParameter[] parameters)
        {
            using var conn = GetConnection();
            EnsureTablesExist(conn, query);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddRange(parameters);

            using var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public DbResult ExecuteNonQuery(string sql, params DuckDBParameter[] parameters)
        {
            try
            {
                using var conn = GetConnection();

                if (IsCreateTableStatement(sql))
                    RegisterTableFromSql(sql);
                else
                    EnsureTablesExist(conn, sql);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);

                int affected = cmd.ExecuteNonQuery();
                return new DbResult
                {
                    Success = true,
                    RowsAffected = affected
                };
            }
            catch (Exception ex)
            {
                string errorMsg = ex switch
                {
                    DuckDBException dbEx => $"数据库错误({dbEx.SqlState}): {dbEx.Message}",  // $位置修复
                    _ => $"系统异常: {ex.Message}"  // 默认分支修复
                };

                // 增强错误提示
                if (ex.Message.Contains("read-only"))
                    errorMsg += "（数据库可能处于只读模式）";
                else if (ex.Message.Contains("primary key"))
                    errorMsg += "（主键冲突或重复插入）";
                else if (ex.Message.Contains("syntax error"))  // 额外添加语法错误识别
                    errorMsg += "（SQL语法错误）";

                return new DbResult
                {
                    Success = false,
                    ErrorMessage = errorMsg
                };
            }
        }

        private void EnsureTablesExist(DuckDBConnection conn, string sql)
        {
            var tables = ExtractTablesFromQuery(sql);
            foreach (var table in tables.Where(t => TableSchemas.ContainsKey(t)))
            {
                CreateTableIfNotExists(conn, table);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        private void CreateTableIfNotExists(DuckDBConnection conn, string tableName)
        {
            lock (_connectionLock)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                SELECT EXISTS (
                    SELECT 1 
                    FROM information_schema.tables 
                    WHERE table_name = '{tableName}'
                )";
                var exists = (bool)cmd.ExecuteScalar();

                if (!exists && TableSchemas.TryGetValue(tableName, out var schema))
                {
                    cmd.CommandText = schema;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<string> ExtractTablesFromQuery(string query)
        {
            // 忽略注释和字符串字面量
            var cleanSql = Regex.Replace(query, @"--.*?$|/\*.*?\*/|'[^']*'", "",
                RegexOptions.Multiline | RegexOptions.Singleline);

            var matches = Regex.Matches(cleanSql,
                @"\b(?:FROM|JOIN|INTO|UPDATE)\s+(\w+)",
                RegexOptions.IgnoreCase);

            return matches
                .Select(m => m.Groups[1].Value.ToLower())
                .Distinct()
                .ToList();
        }

        private bool IsCreateTableStatement(string sql)
        {
            return Regex.IsMatch(sql, @"^\s*CREATE\s+TABLE\b",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        private void RegisterTableFromSql(string createSql)
        {
            var match = Regex.Match(createSql,
                @"CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?(\w+)",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                RegisterTableSchema(match.Groups[1].Value, createSql);
            }
        }

        public void Dispose() { }
    }

    public class DbResult
    {
        public bool Success { get; set; }      // 执行是否成功
        public int RowsAffected { get; set; }  // 影响行数（成功时有效）
        public string ErrorMessage { get; set; } // 错误详情（失败时有效）
    }
}