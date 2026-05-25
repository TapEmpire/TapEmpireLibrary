using System;
using Io.AppMetrica;
using R3;

namespace TapEmpire.Services
{
    internal class AdsMetricaModule : IDisposable
    {
        private readonly IDisposable _subscription;

        public AdsMetricaModule(IAdsService adsService)
        {
            _subscription = adsService.OnImpression.Subscribe(OnImpression);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnImpression(AdImpressionData data)
        {
            var rev = new AdRevenue(data.Revenue, data.Currency)
            {
                AdType = MapToAdType(data.Format),
                AdNetwork = data.Mediation == AdNetwork.Admob ? "Admob_Native" : data.Network,
                AdUnitId = data.AdUnitId,
                AdPlacementName = data.Placement,
            };
            AppMetrica.ReportAdRevenue(rev);
        }

        private static AdType MapToAdType(AdFormat format) => format switch
        {
            AdFormat.Interstitial => AdType.Interstitial,
            AdFormat.Rewarded => AdType.Rewarded,
            AdFormat.Banner => AdType.Banner,
            AdFormat.Mrec => AdType.Mrec,
            _ => AdType.Other,
        };
    }
}
