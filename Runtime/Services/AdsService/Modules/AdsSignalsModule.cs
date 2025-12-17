using TapEmpire.Modules;
using Zenject;
using R3;
using System.Linq;
using Firebase.Analytics;
using System.Collections.Generic;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public class AdsSignalsModule : IServiceModule
    {
        private IProgressService _progressService;
        private AdsSettings _settings = null;

        private bool _isRevenueEnough = false;
        private float _currentRevenue = 0.0f;
        private double _batchedRevenue = 0.0f;
        private BatchedData[] _batchedData = null;
        private CompositeDisposable _disposables = new();

        public AdsSignalsModule(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
            _settings = diContainer.Resolve<IAdsService>().Settings;

            _currentRevenue = _progressService.GetAdRevenue();
            CheckIsRevenueEnough();

            _batchedData = InitializedBatchedData();

            AnalyticsManager.OnAdPayed += OnAdPayed;

            // var iapService = diContainer.Resolve<IIapService>();
            // iapService.OnPurchaseSuccess.Subscribe(OnPurchase).AddTo(_disposables);
            // iapService.OnPurchaseRestored.Subscribe(OnPurchase).AddTo(_disposables);
        }

        public void Dispose()
        {
            AnalyticsManager.OnAdPayed -= OnAdPayed;
            _disposables.Dispose();
        }

        private void OnPurchase(string purchaseId)
        {
        }

        private void OnAdPayed(string adType, string network, string mediation, AdFormat format, double price,
            string currencyCode, string unitId)
        {
            if (currencyCode == "USD")
            {
                OnBatchedRevenue(price, _batchedData[(int)BatchAnalyticsType.Firebase]);

#if TEL_META
                if (_settings.AdsAnalyticsSettings.EnableMeta)
                {
                    OnBatchedRevenue(price, _batchedData[(int)BatchAnalyticsType.Facebook]);
                    OnAdRevenue(price);
                }
#endif
            }
        }

#if TEL_META
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
                    Facebook.Unity.FB.LogAppEvent(layer.Name, valueToSum: (float)newRevenue,
                    parameters: new Dictionary<string, object>
                    {
                        { "fb_currency", "USD" }
                    });
                }
            }

            _currentRevenue = newRevenue;
            CheckIsRevenueEnough();
        }
#endif

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

#if TEL_META
        private void LogBatchedFacebook(double revenue)
        {
            if (_settings.AdsAnalyticsSettings.EnableMeta)
            {
                Facebook.Unity.FB.LogAppEvent("ad_revenue_batched", valueToSum: (float)revenue,
                    parameters: new Dictionary<string, object>
                    {
                        { "fb_currency", "USD" }
                    });
            }
        }
#endif

        private BatchedData[] InitializedBatchedData()
        {
            var batchedData = new BatchedData[] {
                new BatchedData() {
                    BatchType = _settings.AdsAnalyticsSettings.BatchType,
                    Threshold = _settings.AdsAnalyticsSettings.Threshold,
                    Postfix = "",
                    Callback = this.LogBatchedFirebase,
                },
#if TEL_META
                new BatchedData() {
                    BatchType = _settings.AdsAnalyticsSettings.BatchTypeMeta,
                    Threshold = _settings.AdsAnalyticsSettings.ThresholdMeta,
                    Postfix = "Meta",
                    Callback = this.LogBatchedFacebook,
                }
#endif
            };

            batchedData.ForEach(batchedData => batchedData.Initialize(_progressService));

            return batchedData;
        }
    }

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
}