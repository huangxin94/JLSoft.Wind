using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;
using NLog;
using LogLevel = Sunny.UI.LogLevel;

namespace JLSoft.Wind.Logs
{
    public static class LogManager
    {
        private static UIRichTextBox _logBox;
        private static readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private static readonly object _queueLock = new object();
        private static System.Windows.Forms.Timer _updateTimer;

        public static void Initialize(UIRichTextBox logBox)
        {
            _logBox = logBox;
            _logBox.ReadOnly = true;

            // 创建定时器处理UI更新
            _updateTimer = new System.Windows.Forms.Timer { Interval = 100 }; // 每100毫秒更新一次
            _updateTimer.Tick += ProcessLogQueue;
            _updateTimer.Start();
        }

        public static void Log(string message, Sunny.UI.LogLevel level = Sunny.UI.LogLevel.Info, string loggerName = null)
        {
            // 1. UI日志（原有逻辑）
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Level = level
            };
            lock (_queueLock)
            {
                _logQueue.Enqueue(entry);
            }

            // 2. NLog日志

            var nlogger = loggerName == null
                ? NLog.LogManager.GetCurrentClassLogger()
                : NLog.LogManager.GetLogger(loggerName);
            nlogger.Log(MapToNLogLevel(level), message);
        }

        private static void ProcessLogQueue(object sender, EventArgs e)
        {
            if (_logQueue.IsEmpty || _logBox == null) return;

            lock (_queueLock)
            {
                while (!_logQueue.IsEmpty)
                {
                    if (_logQueue.TryDequeue(out var entry))
                    {
                        SafeAppendLog(entry);
                    }
                }
            }
        }

        private static void SafeAppendLog(LogEntry entry)
        {
            if (_logBox.InvokeRequired)
            {
                _logBox.Invoke((Action<LogEntry>)SafeAppendLog, entry);
                return;
            }

            string timestamp = entry.Timestamp.ToString("HH:mm:ss.fff");
            string fullMessage = $"[{timestamp}] {entry.Message}";

            _logBox.SelectionStart = _logBox.TextLength;
            _logBox.SelectionColor = GetLevelColor(entry.Level);
            _logBox.AppendText(fullMessage + Environment.NewLine);
            _logBox.SelectionStart = _logBox.TextLength;
            _logBox.ScrollToCaret();
        }

        // 清除所有日志
        public static void Clear()
        {
            if (_logBox.InvokeRequired)
            {
                _logBox.Invoke(new Action(Clear));
                return;
            }

            lock (_queueLock)
            {
                while (!_logQueue.IsEmpty)
                {
                    _logQueue.TryDequeue(out _);
                }

                _logBox.Clear();
            }
        }
        /// <summary>
        /// 映射NLog日志级别
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static NLog.LogLevel MapToNLogLevel(Sunny.UI.LogLevel level)
        {
            return level switch
            {
                Sunny.UI.LogLevel.Trace => NLog.LogLevel.Trace,
                Sunny.UI.LogLevel.Debug => NLog.LogLevel.Debug,
                Sunny.UI.LogLevel.Info => NLog.LogLevel.Info,
                Sunny.UI.LogLevel.Warn => NLog.LogLevel.Warn,
                Sunny.UI.LogLevel.Error => NLog.LogLevel.Error,
                Sunny.UI.LogLevel.Fatal => NLog.LogLevel.Fatal,
                _ => NLog.LogLevel.Info
            };
        }
        // 获取日志级别的颜色
        private static Color GetLevelColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => Color.LightGray,
                LogLevel.Debug => Color.Blue,
                LogLevel.Info => Color.Green,
                LogLevel.Warn => Color.Orange,
                LogLevel.Error => Color.Red,
                LogLevel.Fatal => Color.DarkRed,
                _ => UIColor.Black
            };
        }
    }

}