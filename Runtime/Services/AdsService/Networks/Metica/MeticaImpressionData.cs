#if TEL_METICA
using Metica.Ads;

namespace TapEmpire.Services
{
    internal static class MeticaImpressionData
    {
        public static AdImpressionData Create(MeticaAd ad, AdFormat format)
        {
            var placement = string.IsNullOrEmpty(ad.placementTag) ? format.ToString() : ad.placementTag;
            return new AdImpressionData(
                AdNetwork.Max,
                format,
                ad.networkName,
                ad.adUnitId,
                placement,
                ad.revenue ?? 0,
                "USD",
                ad.revenuePrecision);
        }
    }
}
#endif
