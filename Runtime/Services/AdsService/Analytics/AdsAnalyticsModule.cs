using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services
{
    public enum BatchAnalyticsType
    {
        Firebase,
        Facebook,
    }

    public class BatchedData
    {
        public double Revenue;
        public bool IsBatchedOnce;
        public BatchType BatchType;
        public double Threshold;
        public string Postfix;
        public System.Action<double> Callback;

        public void Initialize(IProgressService progressService)
        {
            Revenue = progressService.GetAdRevenueBatched(Postfix);
            IsBatchedOnce = BatchType == BatchType.Once && progressService.GetOnceBatched(Postfix);
        }
    }

    public class AdsAnalyticsModule
    {
        private readonly DiContainer _diContainer = null;
        private IAnalyticsService _analyticsService = null;
        private IProgressService _progressService = null;

        private AdsSettings _settings = null;
        private System.DateTime _revenueWindowEnd;
        private bool _isRevenueEnough = false;
        private float _currentRevenue = 0.0f;
        private double _batchedRevenue = 0.0f;
        private bool _isBatchedOnce = false;
        private BatchedData[] _batchedData = null;
        private CompositeDisposable _disposables = new();

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

            _batchedData = InitializedBatchedData();

            adsService.OnAdClickedEvent += OnAdClickedEvent;
            adsService.OnAdDisplayedRewardEvent += OnAdShowing;
            adsService.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;
            adsService.OnInterstitialAdShowRequested += OnInterstitialAdShowRequested;
            AnalyticsManager.OnAdPayed += OnAdPayed;

            adsService.OnAdsInitialized.Subscribe(_ =>
                _analyticsService.SetUserProperty(AdsAnalyticsEvents.IsMeticaEnabled, adsService.IsMeticaEnabled.ToString(), true))
                .AddTo(_disposables);
        }

        public void OnRelease()
        {
            var adsService = _diContainer.Resolve<IAdsService>();

            adsService.OnAdClickedEvent -= OnAdClickedEvent;
            adsService.OnAdDisplayedRewardEvent -= OnAdShowing;
            adsService.OnAdReceivedRewardEvent -= OnAdReceivedRewardEvent;
            adsService.OnInterstitialAdShowRequested -= OnInterstitialAdShowRequested;
            AnalyticsManager.OnAdPayed -= OnAdPayed;

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

        private void OnAdPayed(string adType, string network, string mediation, AdFormat format, double price,
            string currencyCode, string unitId)
        {
            if (mediation != "AdMob Mediation")
            {
                OnBatchedRevenue(price, _batchedData[(int)BatchAnalyticsType.Firebase]);

                if (_settings.EnableMeta)
                {
                    OnBatchedRevenue(price, _batchedData[(int)BatchAnalyticsType.Facebook]);
                    OnAdRevenue(price);
                }
            }

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

        private void OnAdRevenue(double price)
        {
            if (_isRevenueEnough)
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
                    Facebook.Unity.FB.LogAppEvent(layer.Name);
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

        private void OnBatchedRevenue(double price, BatchedData batchedData)
        {
            switch (batchedData.BatchType)
            {
                case BatchType.Taichi:
                    UpdateBatchedRevenue(price, batchedData);
                    break;
                case BatchType.Once:
                    if (UpdateBatchedRevenue(price, batchedData))
                    {
                        _progressService.SetOnceBatched(batchedData.Postfix);
                        batchedData.IsBatchedOnce = true;
                    }
                    break;
                case BatchType.None:
                default:
                    return;
            }
        }

        private bool UpdateBatchedRevenue(double price, BatchedData batchedData)
        {
            batchedData.Revenue += price;

            if (batchedData.Revenue >= batchedData.Threshold || batchedData.IsBatchedOnce)
            {
                batchedData.Callback(batchedData.Revenue);
                _progressService.ClearAdRevenueBatched(batchedData.Postfix);
                batchedData.Revenue = 0.0;
                return true;
            }
            else
            {
                _progressService.SetAdRevenueBatched(_batchedRevenue, batchedData.Postfix);
                return false;
            }
        }

        private void LogBatchedFirebase(double revenue)
        {
            var impressionParameters = new[] {
                new Parameter(FirebaseAnalytics.ParameterValue, revenue),
                new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
            };
            FirebaseAnalytics.LogEvent("ad_revenue_batched", impressionParameters);
        }

        private void LogBatchedFacebook(double revenue)
        {
            Facebook.Unity.FB.LogAppEvent("ad_revenue_batched", valueToSum: (float)revenue,
                parameters: new Dictionary<string, object>
                {
                    { "fb_currency", "USD" }
                });
        }

        private BatchedData[] InitializedBatchedData()
        {
            var batchedData = new BatchedData[2] {
                new BatchedData() {
                    BatchType = _settings.AdsAnalyticsSettings.BatchType,
                    Threshold = _settings.AdsAnalyticsSettings.Threshold,
                    Postfix = "",
                    Callback = this.LogBatchedFirebase,
                },
                new BatchedData() {
                    BatchType = _settings.AdsAnalyticsSettings.BatchTypeMeta,
                    Threshold = _settings.AdsAnalyticsSettings.ThresholdMeta,
                    Postfix = "Meta",
                    Callback = this.LogBatchedFacebook,
                }
            };

            batchedData.ForEach(batchedData => batchedData.Initialize(_progressService));

            return batchedData;
        }
    }
}