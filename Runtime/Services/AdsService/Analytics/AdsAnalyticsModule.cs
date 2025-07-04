using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Newtonsoft.Json.Linq;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services
{
    public class AdsAnalyticsModule
    {
        private readonly DiContainer _diContainer = null;
        private IAnalyticsService _analyticsService = null;
        private IProgressService _progressService = null;

        private AdsSettings _settings = null;
        private System.DateTime _revenueWindowEnd;
        private bool _isRevenueEnough = false;
        private float _currentRevenue = 0.0f;

        public AdsAnalyticsModule(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _analyticsService = _diContainer.Resolve<IAnalyticsService>();
            _progressService = _diContainer.Resolve<IProgressService>();

            _settings = _diContainer.Resolve<IAdsService>().Settings;
        }

        public void SetGlobalParameters()
        {
        }

        public void Initialize()
        {
            var adsService = _diContainer.Resolve<IAdsService>();

            _revenueWindowEnd = PlayerPrefsUtility.GetFirstLaunchDate().AddDays(1);
            _currentRevenue = _progressService.GetAdRevenue();
            CheckIsRevenueEnough();

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
            var levelsCompleted = _progressService.GetLevelProgress();

            _analyticsService.SetUserProperty(AdsAnalyticsParameters.AdsWatched, adsWatchedCount);
            _analyticsService.LogEvent(AdsAnalyticsEvents.AdsWatched, new Dictionary<string, object>{
                { "placement", adPlacement },
                { "network", adData.Network },
                { "mediation", adData.Mediation },
                { "format", adData.Format.ToString() },
                { "level", levelsCompleted },
            });
        }

        private void OnAdPayed(string adType, string network, string mediation, AdFormat format, double price)
        {
            OnAdRevenue(price);

            var levelsCompleted = _progressService.GetLevelProgress();
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
            }

            _analyticsService.LogEvent(AdsAnalyticsStrings.AdsPlacements, parameters);
        }

        private void OnAdRevenue(double price)
        {
            if (_isRevenueEnough || System.DateTime.UtcNow < _revenueWindowEnd)
            {
                return;
            }

            var newRevenue = _progressService.UpdateAdRevenue(price);

            foreach (var layer in _settings.RevenueLayers)
            {
                if (layer.Value > newRevenue)
                    break;

                if (layer.Value > _currentRevenue)
                {
                    if (TapEmpire.Services.FirebaseService.IsInitializedDeprecated)
                    {
                        UnityEngine.Debug.LogError(layer.Name);
                        FirebaseAnalytics.LogEvent(layer.Name);
                    }
                }
            }

            _currentRevenue = newRevenue;
            CheckIsRevenueEnough();
        }

        private void CheckIsRevenueEnough()
        {
            if (_currentRevenue >= _settings.RevenueLayers.Last().Value)
            {
                _isRevenueEnough = true;
            }
        }
    }
}