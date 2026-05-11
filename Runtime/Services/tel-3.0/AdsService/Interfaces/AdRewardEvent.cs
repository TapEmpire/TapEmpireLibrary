namespace TapEmpire.Experimental
{
    public readonly struct AdRewardEvent
    {
        public readonly string RewardType;
        public readonly int Amount;

        public AdRewardEvent(string rewardType, int amount)
        {
            RewardType = rewardType;
            Amount = amount;
        }
    }
}
