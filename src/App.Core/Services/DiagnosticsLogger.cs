using System;
using System.IO;

namespace FastBoard.Core.Services
{
    public static class DiagnosticsLogger
    {
        private static readonly object _lock = new();
        private static readonly string _dir = Path.Combine("dist");
        private static readonly string _path = Path.Combine(_dir, "diagnostics.log");

        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message, Exception? ex = null) => Write("ERROR", message + (ex!=null?"\n"+ex:""));

        private static void Write(string level, string message)
        {
            try
            {
                lock(_lock)
                {
                    Directory.CreateDirectory(_dir);
                    File.AppendAllText(_path, $"{DateTime.Now:O} [{level}] {message}\n");
                }
            }
            catch { /* ignore logging errors */ }
        }
    }
}
