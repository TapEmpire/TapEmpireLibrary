#if TEL_METICA
using Metica.ADS;

namespace TapEmpire.Experimental
{
    internal static class MeticaImpressionData
    {
        public static AdImpressionData Create(MeticaAd ad, AdFormat format)
        {
            var adInfo = ad.ToAdInfo();
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
#endif
