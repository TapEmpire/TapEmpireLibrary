using System;
using Firebase.Analytics;
using R3;

namespace TapEmpire.Services
{
    internal class AdsFirebaseModule : IDisposable
    {
        private readonly IDisposable _subscription;

        public AdsFirebaseModule(IAdsService adsService)
        {
            _subscription = adsService.OnImpression.Subscribe(OnImpression);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnImpression(AdImpressionData data)
        {
            var platform = data.Mediation == AdNetwork.Admob ? "Admob" : "AppLovin";
            var source = data.Mediation == AdNetwork.Admob ? "Simple Admob" : data.Network;
            var unitName = $"{data.Format}_{data.AdUnitId}";
            var adFormat = $"{platform}_{data.Format}";

            var parameters = new[]
            {
                new Parameter("ad_platform", platform),
                new Parameter("ad_source", source),
                new Parameter("ad_unit_name", unitName),
                new Parameter("ad_format", adFormat),
                new Parameter("value", data.Revenue),
                new Parameter("currency", data.Currency),
                new Parameter("ad_placement", data.Placement.ToLower()),
            };

            FirebaseAnalytics.LogEvent("ad_impression", parameters);
#if UNITY_IOS
            FirebaseAnalytics.LogEvent("paid_ad_impression", parameters);
#endif
        }
    }
}
