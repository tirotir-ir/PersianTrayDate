using Microsoft.Win32;
using System;

namespace PersianTrayDate
{
    public static class ThemeManager
    {
        public static bool IsLightTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize"))
                {
                    if (key == null) return true;
                    object val = key.GetValue("AppsUseLightTheme");
                    if (val == null) return true;
                    return Convert.ToInt32(val) != 0;
                }
            } catch { return true; }
        }

        public static IconTheme AutoPickTheme()
        {
            return IsLightTheme() ? IconTheme.Blue : IconTheme.Purple;
        }
    }
}
