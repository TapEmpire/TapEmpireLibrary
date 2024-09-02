namespace TapEmpire.Services
{
    public static partial class AnalyticsEvents
    {
        public const string SessionStart = "SESSION_START";
        public const string SessionEnd = "SESSION_END";
        public const string LaunchFirstTime = "LAUNCH_FIRST_TIME";
    }

    public static partial class AnalyticsParameters
    {
        public const string InstallYear = "InstallYear";
        public const string InstallDate = "InstallDate";
        public const string DaysAfterInstall = "DaysAfterInstall";
        public const string AdjustAttribution = "Attribution";
        public const string RemoteConfig = "RemoteConfig";
    }
}
