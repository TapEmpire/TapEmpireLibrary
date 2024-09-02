using System.Collections.Generic;
using TEL.Services;
using Zenject;

namespace TapEmpire.Services
{
    public class AdsAnalyticsModule
    {
        private readonly DiContainer _diContainer = null;
        private IAnalyticsService _analyticsService = null;

        public AdsAnalyticsModule(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _analyticsService = _diContainer.Resolve<IAnalyticsService>();
        }

        public void SetGlobalParameters()
        {
        }

        public void Initialize()
        {
            var adsService = _diContainer.Resolve<IAdsService>();

            adsService.OnAdClickedEvent += OnAdClickedEvent;
            adsService.OnAdDisplayedRewardEvent += OnAdShowing;
            adsService.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;
            adsService.OnInterstitialAdShowRequested += OnInterstitialAdShowRequested;
            AnalyticsManager.OnAdPayed += OnAdPayed;
        }

        public void OnRelease()
        {
            var adsService = _diContainer.Resolve<IAdsService>();

            adsService.OnAdClickedEvent -= OnAdClickedEvent;
            adsService.OnAdDisplayedRewardEvent -= OnAdShowing;
            adsService.OnAdReceivedRewardEvent -= OnAdReceivedRewardEvent;
            adsService.OnInterstitialAdShowRequested -= OnInterstitialAdShowRequested;
            AnalyticsManager.OnAdPayed -= OnAdPayed;
        }

        private void OnAdClickedEvent(string adPlacement)
        {
            _analyticsService.LogEvent(AnalyticsEvents.AdsButtonClicked, new Dictionary<string, object>{
                { "placement", adPlacement },
            });
        }

        private void OnAdShowing(string adPlacement)
        {
            _analyticsService.LogEvent(AnalyticsEvents.AdsStarted, new Dictionary<string, object>{
                { "placement", adPlacement },
            });
        }

        private void OnInterstitialAdShowRequested(bool hasAds)
        {
            _analyticsService.LogEvent(AnalyticsEvents.AdsInterstitialCheck, new Dictionary<string, object>{
                { "has_ads", hasAds },
            });
        }

        private void OnAdReceivedRewardEvent(string adPlacement)
        {
            var adData = AnalyticsManager.LastAdData;
            
            var progressService = _diContainer.Resolve<IProgressService>();
            var adsWatchedCount = progressService.UpdateAdsWatchedProgress();
            var levelsCompleted = progressService.GetLevelProgress();
            
            _analyticsService.SetUserProperty(AnalyticsParameters.AdsWatched, adsWatchedCount);
            _analyticsService.LogEvent(AnalyticsEvents.AdsWatched, new Dictionary<string, object>{
                { "placement", adPlacement },
                { "network", adData.Network },
                { "mediation", adData.Mediation },
                { "format", adData.Format },
                { "level", levelsCompleted },
            });
        }

        private void OnAdPayed(string adType, string network, string mediation, AdFormat format, double price)
        {
            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
            _analyticsService.LogEvent(AnalyticsEvents.AdsPayed, new Dictionary<string, object>{
                { "placement", adType },
                { "network", network },
                { "mediation", mediation },
                { "format", format },
                { "price", price },
                { "level", levelsCompleted },
            });
        }
    }
}