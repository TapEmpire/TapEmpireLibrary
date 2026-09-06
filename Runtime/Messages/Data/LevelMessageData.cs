namespace TapEmpire.Messages
{
    public class LevelMessageData : IMessageData
    {
        public int LevelIndex;
    }

    public class StartLevelMessageData : LevelMessageData
    {
    }

    public class EndLevelMessageData : LevelMessageData
    {
    }
}
