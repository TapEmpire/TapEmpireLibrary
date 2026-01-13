using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Modules;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services
{
    public class AdsAnalyticsModule : IServiceModule
    {
        private IAnalyticsService _analyticsService = null;
        private IProgressService _progressService = null;

        private CompositeDisposable _disposables = new();

        public AdsAnalyticsModule(DiContainer diContainer)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _progressService = diContainer.Resolve<IProgressService>();

            var adsService = diContainer.Resolve<IAdsService>();

            adsService.OnAdClickedEvent += OnAdClickedEvent;
            adsService.OnAdDisplayedRewardEvent += OnAdShowing;
            adsService.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;
            adsService.OnInterstitialAdShowRequested += OnInterstitialAdShowRequested;
            AnalyticsManager.OnAdPayed += OnAdPayed;

            Disposable.Create(() =>
            {
                adsService.OnAdClickedEvent -= OnAdClickedEvent;
                adsService.OnAdDisplayedRewardEvent -= OnAdShowing;
                adsService.OnAdReceivedRewardEvent -= OnAdReceivedRewardEvent;
                adsService.OnInterstitialAdShowRequested -= OnInterstitialAdShowRequested;
                AnalyticsManager.OnAdPayed -= OnAdPayed;
            }).AddTo(_disposables);

            adsService.OnAdsInitialized.Subscribe(_ =>
                _analyticsService.SetUserProperty(AdsAnalyticsEvents.IsMeticaEnabled, adsService.IsMeticaEnabled.ToString(), true))
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnAdClickedEvent(string adPlacement)
        {
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsButtonClicked, new Dictionary<string, object>{
                { "placement", adPlacement },
            });
        }

        private void OnAdShowing(string adPlacement)
        {
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsStarted, new Dictionary<string, object>{
                { "placement", adPlacement },
            });
        }

        private void OnInterstitialAdShowRequested(bool hasAds)
        {
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsInterstitialCheck, new Dictionary<string, object>{
                { "has_ads", hasAds },
            });
        }

        private void OnAdReceivedRewardEvent(string adPlacement)
        {
            var adData = AnalyticsManager.LastAdData;

            var adsWatchedCount = _progressService.UpdateAdsWatchedProgress();
            var levelsCompleted = _progressService.GetVisualProgress();

            _analyticsService.SetUserProperty(AdsAnalyticsParameters.AdsWatched, adsWatchedCount);
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsWatched, new Dictionary<string, object>{
                { "placement", adPlacement },
                { "network", adData.Network },
                { "mediation", adData.Mediation },
                { "format", adData.Format.ToString() },
                { "level", levelsCompleted },
            });
        }

        private void OnAdPayed(string adType, string network, string mediation, AdFormat format, double price,
            string currencyCode, string unitId)
        {
            var levelsCompleted = _progressService.GetVisualProgress();
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsPayed, new Dictionary<string, object>{
                { "placement", adType },
                { "network", network },
                { "mediation", mediation },
                { "format", format.ToString() },
                { "price", price },
                { "level", levelsCompleted },
            });

            var parameters = new Dictionary<string, object> { };
            var levelParameter = $"level_{levelsCompleted}";

            if (format == AdFormat.Interstitial)
            {
                parameters.Add(format.ToString(), levelParameter);
            }
            else if (format == AdFormat.Rewarded || format == AdFormat.RewardedInt)
            {
                parameters.Add(format.ToString(), new JObject(new JProperty(adType, levelParameter)));
            }
            else
            {
                parameters.Add(format.ToString(), null);
                adType = string.Empty;
            }

            _analyticsService.LogEvent(AdsAnalyticsStrings.AdsPlacements, parameters);

            _analyticsService.LogAdjustEvent(new Dictionary<string, object>
            {
                { "adjust_event_name", "ad_impression" },
                { "level", levelsCompleted },
                { "ad_platform", mediation },
                { "ad_source", network },
                { "ad_unit_name", unitId },
                { "ad_format", format.ToString() },
                { "ad_placement", adType },
                { "ad_revenue", price },
                { "currency", currencyCode }
            });
        }
    }
}
