using System;

namespace TapEmpire.Utility
{
    public static class CountdownFormatUtility
    {
        public static string Format(TimeSpan remaining)
        {
            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
            return $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
}
