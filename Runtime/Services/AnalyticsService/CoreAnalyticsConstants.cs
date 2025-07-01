namespace TapEmpire.Services.Analytics
{
    public static class GameAnalyticsEvents
    {
        public const string LevelStarted = "LevelStarted";
        public const string LevelCompleted = "LevelCompleted";
        public const string LevelUnlockPressed = "LevelUnlockPressed";
        public const string LevelUnlocked = "LevelUnlocked";
        public const string LevelSkipPressed = "LevelSkipPressed";
        public const string LevelSkipped = "LevelSkipped";
        public const string HintAdded = "HintAdded";
        public const string HintPressed = "HintPressed";
        public const string HintShown = "HintShown";
        public const string TimerShown = "TimerShown";
        public const string TimerRevived = "TimerRevived";
    }
    
    public static class CoreAnalyticsParameters
    {
        public const string LEVEL = "level";
        public const string REASON = "reason";
        public const string CYCLE = "cycle";
    }
    
    public static class CoreAnalyticsReasons
    {
        public const string RETRY = "retry";
        public const string WIN = "win";
        public const string QUIT = "quit";
        public const string LOST = "lost";
    }

    public static class CoreAnalyticsStrings
    {
        public const string GameData = "GameData";
        public const string CommonData = "CommonData";
    }
}