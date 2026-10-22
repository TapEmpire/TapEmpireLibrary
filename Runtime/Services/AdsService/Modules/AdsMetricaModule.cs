using System;
using R3;

namespace TapEmpire.Services
{
    internal class AdsMetricaModule : IDisposable
    {
        private readonly IDisposable _subscription;

        public AdsMetricaModule(IAdsService adsService)
        {
            _subscription = adsService.OnImpressionUnsafe.Subscribe(OnImpression);
        }

        public void Dispose() => _subscription?.Dispose();

        private void OnImpression(AdImpressionData data)
        {
            var rev = new Io.AppMetrica.AdRevenue(data.Revenue, data.Currency)
            {
                AdType = MapToAdType(data.Format),
                AdNetwork = data.Mediation == AdNetwork.Admob ? "Admob_Native" : data.Network,
                AdUnitId = data.AdUnitId,
                AdPlacementName = data.Placement,
            };
            Io.AppMetrica.AppMetrica.ReportAdRevenue(rev);
        }

        private static Io.AppMetrica.AdType MapToAdType(AdFormat format) => format switch
        {
            AdFormat.Interstitial => Io.AppMetrica.AdType.Interstitial,
            AdFormat.Rewarded => Io.AppMetrica.AdType.Rewarded,
            AdFormat.Banner => Io.AppMetrica.AdType.Banner,
            AdFormat.Mrec => Io.AppMetrica.AdType.Mrec,
            _ => Io.AppMetrica.AdType.Other,
        };
    }
}
