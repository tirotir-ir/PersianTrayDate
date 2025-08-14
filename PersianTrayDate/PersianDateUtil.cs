using System;
using System.Globalization;
using System.Text;

namespace PersianTrayDate
{
    public static class PersianDateUtil
    {
        private static readonly PersianCalendar Pc = new PersianCalendar();

        public static readonly string[] MonthNames = new string[]
        {
            "فروردین","اردیبهشت","خرداد","تیر","مرداد","شهریور",
            "مهر","آبان","آذر","دی","بهمن","اسفند"
        };

        public class PersianInfo
        {
            public string Day { get; set; }
            public string MonthName { get; set; }
            public string Year { get; set; }
            public string DayOfWeekName { get; set; }
            public int WeekNumber { get; set; }
            public PersianInfo(string day, string monthName, string year, string dayOfWeekName, int weekNumber)
            { Day=day; MonthName=monthName; Year=year; DayOfWeekName=dayOfWeekName; WeekNumber=weekNumber; }
        }

        public static PersianInfo GetNowInfo(NumeralMode numerals = NumeralMode.Persian) => GetInfo(DateTime.Now, numerals);

        public static PersianInfo GetInfo(DateTime dt, NumeralMode numerals = NumeralMode.Persian)
        {
            int y = Pc.GetYear(dt);
            int m = Pc.GetMonth(dt);
            int d = Pc.GetDayOfMonth(dt);
            int dayOfYear = Pc.GetDayOfYear(dt);
            int week = ((dayOfYear - 1) / 7) + 1;

            string day = numerals == NumeralMode.Persian ? ToPersianDigits(d) : d.ToString();
            string year = numerals == NumeralMode.Persian ? ToPersianDigits(y) : y.ToString();
            string monthName = MonthNames[m - 1];
            string dayOfWeekName = GetPersianDayName(dt.DayOfWeek);
            return new PersianInfo(day, monthName, year, dayOfWeekName, week);
        }

        public static string ToPersianDigits(int n) => ToPersianDigits(n.ToString());
        public static string ToPersianDigits(string input)
        {
            char[] pd = new char[] { '۰','۱','۲','۳','۴','۵','۶','۷','۸','۹' };
            var sb = new StringBuilder(input.Length);
            foreach (char ch in input)
                sb.Append(ch>='0'&&ch<='9' ? pd[ch-'0'] : ch);
            return sb.ToString();
        }

        public static string ToLatinDigits(string input)
        {
            char[] pd = new char[] { '۰','۱','۲','۳','۴','۵','۶','۷','۸','۹' };
            var sb = new StringBuilder(input.Length);
            foreach (char ch in input)
            {
                int idx = Array.IndexOf(pd, ch);
                sb.Append(idx >= 0 ? (char)('0' + idx) : ch);
            }
            return sb.ToString();
        }

        public static string GetPersianDayName(DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Saturday: return "شنبه";
                case DayOfWeek.Sunday: return "یکشنبه";
                case DayOfWeek.Monday: return "دوشنبه";
                case DayOfWeek.Tuesday: return "سه‌شنبه";
                case DayOfWeek.Wednesday: return "چهارشنبه";
                case DayOfWeek.Thursday: return "پنجشنبه";
                case DayOfWeek.Friday: return "جمعه";
                default: return "—";
            }
        }
    }
}
