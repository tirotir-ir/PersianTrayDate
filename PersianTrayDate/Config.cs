using System;
using System.IO;
using System.Text.Json;

namespace PersianTrayDate
{
    public class AppConfig
    {
        public bool StartWithWindows { get; set; } = true;
        public bool TwoDigitDay { get; set; } = false;
        public bool ShowWeekNumber { get; set; } = true;
        public IconTheme Theme { get; set; } = IconTheme.Blue;
        public bool AutoTheme { get; set; } = false;
        public NumeralMode Numerals { get; set; } = NumeralMode.Persian;
        public WeekStartDay WeekStart { get; set; } = WeekStartDay.Saturday;

        public bool TimerPlaySound { get; set; } = true;
        public bool AlarmPlaySound { get; set; } = true;
        public bool Logging { get; set; } = false;
    }

    public static class Config
    {
        private static string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersianTrayDate");
        private static string _file = Path.Combine(_folder, "config.json");

        public static AppConfig Current = new AppConfig();
        public static void Load()
        {
            try
            {
                if (File.Exists(_file))
                {
                    var json = File.ReadAllText(_file);
                    var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                    if (cfg != null) Current = cfg;
                }
            } catch { }
            Logger.Enabled = Current.Logging;
        }
        public static void Save()
        {
            try
            {
                if (!Directory.Exists(_folder)) Directory.CreateDirectory(_folder);
                var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_file, json);
            } catch { }
            Logger.Enabled = Current.Logging;
        }
        public static string ConfigPath() => _file;
        public static string DataFolder() => _folder;
    }
}
