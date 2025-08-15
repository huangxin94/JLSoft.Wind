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
                logger.Info("NLog��ʼ�����");

                DuckDBConnectionString = GetDuckDBConnectionString();

                // ��ʼ�����ݿ�
                InitializeDatabase();

                // To customize application configuration such as set high DPI settings or default font,  
                // see https://aka.ms/applicationconfiguration.  
                ApplicationConfiguration.Initialize();


                Application.Run(new MainForm());
            }catch(Exception ex)
            {
                logger.Error(ex, "Ӧ�ó�������ʧ��");
                MessageBox.Show($"Ӧ�ó�������ʧ��: {ex.Message}", "���ش���",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static void InitializeDatabase()
        {
            try
            {
                logger.Info("��ʼ���ݿ��ʼ��");

                // �������ݿ�����
                using (var connection = new DuckDBConnection(DuckDBConnectionString))
                {
                    connection.Open();
                    logger.Info("�ɹ��������ݿ�");

                    // ע�ᲢǨ�����ݿ�
                    DuckDbORM.RegisterModels();
                    DuckDbORM.MigrateDatabase(connection);
                }

                logger.Info("���ݿ��ʼ�����");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "���ݿ��ʼ��ʧ��");
                MessageBox.Show($"���ݿ��ʼ��ʧ��: {ex.Message}\n\nӦ�ÿ����޷���������",
                    "���ݿ����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// ���ݿ������ַ�����ȡ
        /// </summary>
        /// <returns></returns>
        private static string GetDuckDBConnectionString()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var connectionString = configuration.GetConnectionString("DuckDB");

            // �������룺����·�����Զ�����Ŀ¼
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