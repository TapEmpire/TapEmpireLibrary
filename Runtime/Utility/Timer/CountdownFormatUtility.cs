using System;

namespace TapEmpire.Utility
{
    public static class CountdownFormatUtility
    {
        public static string Format(double remainingSeconds)
        {
            var span = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays}d {span.Hours}h";
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
            return $"{(int)span.TotalMinutes:D2}:{span.Seconds:D2}";
        }
    }
}
