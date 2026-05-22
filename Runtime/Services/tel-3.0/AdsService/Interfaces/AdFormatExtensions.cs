namespace TapEmpire.Experimental
{
    public static class AdFormatExtensions
    {
        public static bool IsExpensive(this AdFormat format)
        {
            return format == AdFormat.Interstitial || format == AdFormat.Rewarded;
        }
    }
}
