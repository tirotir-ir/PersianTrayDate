using Microsoft.Win32;

namespace PersianTrayDate
{
    public static class StartupManager
    {
        private const string RUN_KEY = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string APP_NAME = "PersianTrayDate";

        public static bool IsEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
                    return key != null && key.GetValue(APP_NAME) != null;
            } catch { return false; }
        }
        public static void Enable(string exePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                    if (key != null) key.SetValue(APP_NAME, "\""+exePath+"\"");
            } catch { }
        }
        public static void Disable()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                    if (key != null && key.GetValue(APP_NAME) != null) key.DeleteValue(APP_NAME);
            } catch { }
        }
    }
}
