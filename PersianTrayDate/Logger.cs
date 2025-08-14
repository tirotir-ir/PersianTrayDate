using System;
using System.IO;

namespace PersianTrayDate
{
    public static class Logger
    {
        private static string _file;
        public static bool Enabled { get; set; } = false;

        static Logger()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersianTrayDate");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _file = Path.Combine(folder, "log.txt");
        }

        public static void Log(string msg)
        {
            if (!Enabled) return;
            try { File.AppendAllText(_file, DateTime.Now.ToString("u") + " " + msg + Environment.NewLine); } catch {}
        }
    }
}
