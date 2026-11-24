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
            var precision = value.Precision switch
            {
                AdValue.PrecisionType.Precise => "exact",
                AdValue.PrecisionType.PublisherProvided => "publisher_defined",
                AdValue.PrecisionType.Estimated => "estimated",
                _ => "undefined",
            };
            return new AdImpressionData(
                AdNetwork.Admob,
                format,
                "AdMob",
                adUnitId,
                placement,
                value.Value / 1_000_000d,
                value.CurrencyCode,
                precision,
                string.Empty);
        }
    }
}
