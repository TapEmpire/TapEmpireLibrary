namespace TEL.GraphTool.Data
{
    public class UnlockUserData : IUserData
    {
        public UnlockType? UnlockType = null;
        public int? Cost = null;
        public int? Coins = null;
    }

    public enum UnlockType
    {
        None,
        Ads,
        Ticket,
        Money
    }
}