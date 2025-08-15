using System.Data.Common;
using System.Runtime.InteropServices;
using DuckDB.NET.Data;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services.DuckDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using Logger = NLog.Logger;
using LogManager = NLog.LogManager;


namespace JLSoft.Wind
{
    internal static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static string DuckDBConnectionString { get; private set; }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>  
        ///  The main entry point for the application.  
        /// </summary>  
        [STAThread]
        static void Main()
        {
            try
            {


                if (Environment.OSVersion.Version.Major >= 6)
                    SetProcessDPIAware();
                logger.Info("NLog初始化完成");

                DuckDBConnectionString = GetDuckDBConnectionString();

                // 初始化数据库
                InitializeDatabase();

                // To customize application configuration such as set high DPI settings or default font,  
                // see https://aka.ms/applicationconfiguration.  
                ApplicationConfiguration.Initialize();


                Application.Run(new MainForm());
            }catch(Exception ex)
            {
                logger.Error(ex, "应用程序启动失败");
                MessageBox.Show($"应用程序启动失败: {ex.Message}", "严重错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static void InitializeDatabase()
        {
            try
            {
                logger.Info("开始数据库初始化");

                // 创建数据库连接
                using (var connection = new DuckDBConnection(DuckDBConnectionString))
                {
                    connection.Open();
                    logger.Info("成功连接数据库");

                    // 注册并迁移数据库
                    DuckDbORM.RegisterModels();
                    DuckDbORM.MigrateDatabase(connection);
                }

                logger.Info("数据库初始化完成");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "数据库初始化失败");
                MessageBox.Show($"数据库初始化失败: {ex.Message}\n\n应用可能无法正常工作",
                    "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// 数据库连接字符串获取
        /// </summary>
        /// <returns></returns>
        private static string GetDuckDBConnectionString()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var connectionString = configuration.GetConnectionString("DuckDB");

            // 新增代码：解析路径并自动创建目录
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            if (builder.TryGetValue("Data Source", out var dataSourceObj))
            {
                var dataSource = dataSourceObj.ToString();
                var fullPath = Path.GetFullPath(dataSource);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Console.WriteLine($"Created directory: {directory}");
                }
            }

            return connectionString;
        }
    }
}