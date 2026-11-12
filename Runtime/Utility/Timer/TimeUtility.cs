namespace TapEmpire.Utility
{
    public static class TimeUtility
    {
        public static string FormatMinutes(int seconds)
        {
            return $"{seconds / 60:D2}:{seconds % 60:D2}";
        }
    }
}
