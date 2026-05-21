using GoogleMobileAds.Api;

namespace TapEmpire.Experimental
{
    internal static class AdmobImpressionData
    {
        public static AdImpressionData Create(
            AdValue value,
            AdFormat format,
            string adUnitId,
            string placement = "")
        {
            if (string.IsNullOrEmpty(placement)) placement = format.ToString();
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
