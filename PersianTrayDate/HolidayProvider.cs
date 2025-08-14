using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PersianTrayDate
{
    public static class HolidayProvider
    {
        public static HashSet<int> GetHolidayDays(int year, int month)
        {
            var result = new HashSet<int>();
            try
            {
                string file = Path.Combine(Config.DataFolder(), "holidays.fa.json");
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    var root = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int[]>>>(json);
                    var yKey = PersianDateUtil.ToPersianDigits(year);
                    var mKey = PersianDateUtil.ToPersianDigits(month);
                    if (root != null && root.TryGetValue(yKey, out var months))
                        if (months != null && months.TryGetValue(mKey, out var days))
                            foreach (var d in days) result.Add(d);
                }
                else
                {
                    if (month == 1) { result.Add(1); result.Add(2); result.Add(3); result.Add(4); }
                }
            } catch { }
            return result;
        }
    }
}
