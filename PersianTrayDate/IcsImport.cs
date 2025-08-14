using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace PersianTrayDate
{
    public class IcsEvent
    {
        public DateTime Start { get; set; }
        public string Summary { get; set; }
    }

    public static class IcsImport
    {
        public static List<IcsEvent> Parse(string path)
        {
            var list = new List<IcsEvent>();
            try
            {
                IcsEvent current = null;
                foreach (var line in File.ReadAllLines(path))
                {
                    var l = line.Trim();
                    if (l == "BEGIN:VEVENT") current = new IcsEvent();
                    else if (l == "END:VEVENT") { if (current != null) { list.Add(current); current = null; } }
                    else if (current != null)
                    {
                        if (l.StartsWith("DTSTART"))
                        {
                            int idx = l.IndexOf(':');
                            if (idx > 0)
                            {
                                var val = l.Substring(idx + 1).Trim().Replace("Z", "");
                                if (DateTime.TryParseExact(val, "yyyyMMdd'T'HHmmss", null,
                                    DateTimeStyles.AssumeLocal, out var dt))
                                    current.Start = dt;
                            }
                        }
                        else if (l.StartsWith("SUMMARY:"))
                        {
                            current.Summary = l.Substring(8).Trim();
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        public static IcsEvent NextUpcoming(List<IcsEvent> events)
        {
            DateTime now = DateTime.Now;
            IcsEvent best = null;
            foreach (var e in events)
                if (e.Start > now && (best == null || e.Start < best.Start)) best = e;
            return best;
        }
    }
}
