namespace TapEmpire.Experimental
{
    internal static class MaxImpressionData
    {
        public static AdImpressionData Create(MaxSdkBase.AdInfo adInfo, AdFormat format)
        {
            var placement = string.IsNullOrEmpty(adInfo.Placement) ? format.ToString() : adInfo.Placement;
            return new AdImpressionData(
                AdNetwork.Max,
                format,
                adInfo.NetworkName,
                adInfo.AdUnitIdentifier,
                placement,
                adInfo.Revenue,
                "USD",
                adInfo.RevenuePrecision);
        }
    }
}
