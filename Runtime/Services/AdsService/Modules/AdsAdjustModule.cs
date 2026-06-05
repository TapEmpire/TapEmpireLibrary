using System;
using System.Collections.Generic;
using R3;

namespace TapEmpire.Services
{
    internal class AdsAdjustModule : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly AdsSettings _settings;

        public AdsAdjustModule(IAdsService adsService)
        {
            _settings = adsService.Settings;
            _subscription = adsService.OnImpression.Subscribe(OnImpression);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnImpression(AdImpressionData data)
        {
            if (data.Format == AdFormat.Banner && !_settings.AdsAnalyticsSettings.AddBannerRevenue) return;

            var source = data.Mediation == AdNetwork.Admob ? "admob_sdk" : "applovin_max_sdk";
            var partnerParameters = new Dictionary<string, string>
            {
                ["ad_format"] = data.Format.ToString(),
                ["ad_unit_id"] = data.AdUnitId,
            };
            AttributionService.TrackAdRevenue(
                source,
                data.Revenue,
                data.Currency,
                network: data.Network,
                adRevenueUnit: $"{data.Format}_{data.AdUnitId}",
                partnerParameters: partnerParameters);
        }
    }
}
