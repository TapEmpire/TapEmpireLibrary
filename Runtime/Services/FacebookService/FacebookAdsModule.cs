#if TEL_META
using System;
using System.Collections.Generic;
using Facebook.Unity;
using R3;

namespace TapEmpire.Services
{
    internal class FacebookAdsModule : IDisposable
    {
        private readonly IDisposable _subscription;

        public FacebookAdsModule(IAdsService adsService)
        {
            _subscription = adsService.OnImpression.Subscribe(OnImpression);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnImpression(AdImpressionData data)
        {
            var platform = data.Mediation == AdNetwork.Admob ? "Admob" : "AppLovin";
            var source = data.Mediation == AdNetwork.Admob ? "Simple Admob" : data.Network;

            var parameters = new Dictionary<string, object>
            {
                { "ad_platform", platform },
                { "ad_source", source },
                { "ad_format", data.Format.ToString() },
                { "ad_placement_id", data.Placement },
                { "fb_currency", data.Currency },
            };

            if (!string.IsNullOrEmpty(data.Precision)) parameters["precision"] = data.Precision;

            FB.LogAppEvent("ad_impression", valueToSum: (float)data.Revenue, parameters: parameters);
        }
    }
}
#endif
