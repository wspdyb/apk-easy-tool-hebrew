using System;
using System.IO;

namespace APKEasyTool
{
    /// <summary>
    /// Application-level logger that writes errors and events to application.log
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static bool _enabled = false;

        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private static string LogFilePath
        {
            get { return Path.Combine(Variables.GetPath(), "application.log"); }
        }

        public static void WriteLog(string level, string message)
        {
            if (!_enabled) return;

            try
            {
                lock (_lock)
                {
                    string line = string.Format("[{0}] [{1}] {2}{3}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        level,
                        message,
                        Environment.NewLine);

                    File.AppendAllText(LogFilePath, line);
                }
            }
            catch
            {
                // Silently fail - logging should never crash the app
            }
        }

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        public static void Error(string message, Exception ex)
        {
            WriteLog("ERROR", message + Environment.NewLine + ex.ToString());
        }

        public static void Fatal(string message, Exception ex)
        {
            // Fatal always writes, even if disabled
            try
            {
                lock (_lock)
                {
                    string line = string.Format("[{0}] [FATAL] {1}{2}{3}{4}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        message,
                        Environment.NewLine,
                        ex.ToString(),
                        Environment.NewLine);

                    File.AppendAllText(LogFilePath, line);
                }
            }
            catch { }
        }
    }
}
