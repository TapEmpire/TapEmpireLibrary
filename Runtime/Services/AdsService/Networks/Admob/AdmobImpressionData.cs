using GoogleMobileAds.Api;

namespace TapEmpire.Services
{
    internal static class AdmobImpressionData
    {
        public static AdImpressionData Create(
            AdValue value,
            AdFormat format,
            string adUnitId,
            string placement = "")
        {
            placement = string.IsNullOrEmpty(placement) ? format.ToString() : placement;
            return new AdImpressionData(
                AdNetwork.Admob,
                format,
                "AdMob",
                adUnitId,
                placement,
                value.Value / 1_000_000d,
                value.CurrencyCode,
                value.Precision.ToString());
        }
    }
}
