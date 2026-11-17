namespace TapEmpire.Utility
{
    public static class TimeUtility
    {
        public static string FormatMinutes(int seconds)
        {
            return $"{seconds / 60:D2}:{seconds % 60:D2}";
        }

        public static string FormatMinutesWithUnits(int seconds, bool hasSpace = false)
        {
            if (seconds < 60) return $"{seconds}s";

            var separator = hasSpace ? " " : "";
            return $"{seconds / 60}m{separator}{seconds % 60:D2}s";
        }
    }
}
