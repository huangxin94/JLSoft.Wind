using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Logs
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> lazy = new Lazy<Logger>(() => new Logger());
        private readonly StreamWriter writer;
        private readonly object lockObject = new object();

        public static Logger Instance { get { return lazy.Value; } }

        private Logger()
        {
            try
            {
                // 确保日志目录存在
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 创建带日期的日志文件名
                string logFilePath = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyy-MM-dd}.log");

                // 创建文件流（追加模式，自动刷新）
                writer = new StreamWriter(logFilePath, true, Encoding.UTF8) { AutoFlush = true };

                // 记录日志启动信息
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 日志系统已启动");
            }
            catch (Exception ex)
            {
                // 日志初始化失败处理
                Console.WriteLine($"日志初始化失败: {ex.Message}");
                // 可以考虑添加备选日志方式（如事件日志）
            }
        }

        ~Logger()
        {
            Dispose(false);
        }

        public void WriteLine(string message)
        {
            if (writer == null) return;

            lock (lockObject)
            {
                try
                {
                    writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"写入日志失败: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                writer?.Close();
                writer?.Dispose();
            }
        }
    }
}
