using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;

namespace TapEmpire.Experimental
{
    internal class AdsAnalyticsModule : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly IAnalyticsService _analyticsService;
        private readonly IProgressService _progressService;

        private AdImpressionData _lastRewardImpression;

        public AdsAnalyticsModule(
            IAdsService adsService,
            IAnalyticsService analyticsService,
            IProgressService progressService)
        {
            _analyticsService = analyticsService;
            _progressService = progressService;

            adsService.OnAdClicked.Subscribe(OnAdClicked).AddTo(_disposables);
            adsService.OnInterstitialAttempt.Subscribe(OnInterstitialAttempt).AddTo(_disposables);
            adsService.OnReceivedReward.Subscribe(OnReceivedReward).AddTo(_disposables);
            adsService.OnImpression.Subscribe(OnImpression).AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();

        private void OnAdClicked(string placement)
        {
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsButtonClicked, new Dictionary<string, object>
            {
                { "placement", placement },
            });
        }

        private void OnInterstitialAttempt(bool hasAds)
        {
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsInterstitialCheck, new Dictionary<string, object>
            {
                { "has_ads", hasAds },
            });
        }

        private void OnReceivedReward(Unit _)
        {
            var adsWatched = _progressService.UpdateAdsWatchedProgress();
            var level = _progressService.GetVisualProgress();
            var data = _lastRewardImpression;

            _analyticsService.SetUserProperty(AdsAnalyticsParameters.AdsWatched, adsWatched);
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsWatched, new Dictionary<string, object>
            {
                { "placement", data.Placement },
                { "network", data.Network },
                { "mediation", data.Mediation.ToString() },
                { "format", data.Format.ToString() },
                { "level", level },
            });
        }

        private void OnImpression(AdImpressionData data)
        {
            if (data.Format == AdFormat.Rewarded || data.Format == AdFormat.Interstitial)
            {
                _lastRewardImpression = data;
            }

            LogAdsPayed(data);
            LogAdsPlacements(data);
            LogAdjustImpression(data);
        }

        private void LogAdsPayed(AdImpressionData data)
        {
            var level = _progressService.GetVisualProgress();
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsPayed, new Dictionary<string, object>
            {
                { "placement", data.Placement },
                { "network", data.Network },
                { "mediation", data.Mediation.ToString() },
                { "format", data.Format.ToString() },
                { "price", data.Revenue },
                { "level", level },
            });
        }

        private void LogAdsPlacements(AdImpressionData data)
        {
            var level = _progressService.GetVisualProgress();
            var levelParameter = $"level_{level}";
            var parameters = new Dictionary<string, object>();

            if (data.Format == AdFormat.Interstitial || data.Format == AdFormat.Rewarded)
            {
                parameters.Add(data.Format.ToString(), new JObject(new JProperty(data.Placement, levelParameter)));
            }
            else
            {
                parameters.Add(data.Format.ToString(), null);
            }

            _analyticsService.LogEvent(AdsAnalyticsStrings.AdsPlacements, parameters);
        }

        private void LogAdjustImpression(AdImpressionData data)
        {
            var level = _progressService.GetVisualProgress();
            _analyticsService.LogAdjustEvent(new Dictionary<string, object>
            {
                { "adjust_event_name", "ad_impression" },
                { "level", level },
                { "ad_platform", data.Mediation.ToString() },
                { "ad_source", data.Network },
                { "ad_unit_name", data.AdUnitId },
                { "ad_format", data.Format.ToString() },
                { "ad_placement", data.Placement },
                { "ad_revenue", data.Revenue },
                { "currency", data.Currency },
            });
        }
    }
}
