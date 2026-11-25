using System;
using System.Collections.Generic;
using R3;

namespace TapEmpire.Services
{
    internal class CachedAdsModule : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly IAnalyticsService _analyticsService;
        private readonly IProgressService _progressService;

        public CachedAdsModule(
            IInterstitial interstitial,
            IRewarded rewarded,
            IAnalyticsService analyticsService,
            IProgressService progressService)
        {
            _analyticsService = analyticsService;
            _progressService = progressService;

            if (interstitial != null)
            {
                interstitial.OnLoaded.Merge(interstitial.OnRecached)
                    .TakeUntil(interstitial.OnImpression)
                    .Subscribe(LogAdsCached)
                    .AddTo(_disposables);
            }

            rewarded.OnLoaded.Merge(rewarded.OnRecached)
                .TakeUntil(rewarded.OnImpression)
                .Subscribe(LogAdsCached)
                .AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();

        private void LogAdsCached(AdImpressionData data)
        {
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsCached, new Dictionary<string, object>
            {
                { "placement", data.Placement },
                { "network", data.Network },
                { "mediation", data.Mediation.ToString() },
                { "format", data.Format.ToString() },
                { "price", data.Revenue },
                { "precision", data.Precision },
                { "level", _progressService.GetVisualProgress() },
                { "creative_id", data.CreativeId },
            });
        }
    }
}
