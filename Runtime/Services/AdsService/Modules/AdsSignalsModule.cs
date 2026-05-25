using TapEmpire.Modules;
using Zenject;
using R3;
using System.Linq;
using Firebase.Analytics;
using TapEmpire.Utility;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    public class AdsSignalsModule : IServiceModule
    {
        private IProgressService _progressService;
        private AdsSettings _settings = null;

        private BatchedData[] _batchedData = null;
        private LayeredData[] _layeredData = null;
        private CompositeDisposable _disposables = new();

        public AdsSignalsModule(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
            _settings = diContainer.Resolve<IAdsService>().Settings;

            _batchedData = InitializeBatchedData();
            _layeredData = InitializeLayeredData();

            AnalyticsManager.OnAdPayed += OnAdPayed;

#if TEL_META
            if (_settings.AdsAnalyticsSettings.EnableMeta)
            {
                var iapService = diContainer.Resolve<IIapService>();
                iapService.OnPurchaseSuccessDetailed.Subscribe(OnPurchaseDetailed).AddTo(_disposables);
            }
#endif
        }

        public void Dispose()
        {
            AnalyticsManager.OnAdPayed -= OnAdPayed;
            _disposables.Dispose();
        }

#if TEL_META
        private void OnPurchaseDetailed((Product Product, string ProductId, string Placement) data)
        {
            var price = data.Product.metadata.localizedPrice;
            var isoCode = data.Product.metadata.isoCurrencyCode;

            if (isoCode == "USD")
            {
                if (_settings.AdsAnalyticsSettings.AddMetaIapBatched)
                {
                    OnBatchedRevenue((double)price, _batchedData[(int)AdsAnalyticsType.Facebook]);
                }

                if (_settings.AdsAnalyticsSettings.AddMetaIapLayered)
                {
                    OnAdRevenue((double)price, _layeredData[(int)AdsAnalyticsType.Facebook]);
                }
            }
        }
#endif

        private void OnAdPayed(string adType, string network, string mediation, AdFormat format, double price,
            string currencyCode, string unitId)
        {
            if (currencyCode == "USD")
            {
                OnBatchedRevenue(price, _batchedData[(int)AdsAnalyticsType.Firebase]);
                OnAdRevenue(price, _layeredData[(int)AdsAnalyticsType.Firebase]);

#if TEL_META
                if (_settings.AdsAnalyticsSettings.EnableMeta)
                {
                    OnBatchedRevenue(price, _batchedData[(int)AdsAnalyticsType.Facebook]);
                    OnAdRevenue(price, _layeredData[(int)AdsAnalyticsType.Facebook]);
                }
#endif
            }

            OnCounterUpdate(format, currencyCode == "USD" ? price : 0.0);
        }

        private void OnAdRevenue(double price, LayeredData data)
        {
            if (data.IsRevenueEnough)
            {
                return;
            }

            var newRevenue = _progressService.UpdateAdRevenueLayered(price, data.Postfix);

            foreach (var layer in data.RevenueLayers)
            {
                if (layer.Value > newRevenue)
                    break;

                if (layer.Value > data.CurrentRevenue)
                {
                    data.Callback(layer.Name, newRevenue);
                }
            }

            data.CurrentRevenue = newRevenue;
            data.CheckIsRevenueEnough();
        }

        private void LogLayeredFirebase(string name, double revenue) => FirebaseService.LogEvent(name);

#if TEL_META
        private void LogLayeredFacebook(string name, double revenue)
        {
            if (_settings.AdsAnalyticsSettings.EnableMeta)
            {
                Facebook.Unity.FB.LogAppEvent(name, valueToSum: (float)revenue,
                    parameters: new Dictionary<string, object>
                    {
                        { "fb_currency", "USD" }
                    });
            }
        }
#endif

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
                _progressService.SetAdRevenueBatched(batchedData.Revenue, batchedData.Postfix);
                return false;
            }
        }

        private void LogBatchedFirebase(double revenue)
        {
            var impressionParameters = new[] {
                new Parameter(FirebaseAnalytics.ParameterValue, revenue),
                new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
            };
            FirebaseService.LogEvent("ad_revenue_batched", impressionParameters);
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

        private BatchedData[] InitializeBatchedData()
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

        private LayeredData[] InitializeLayeredData()
        {
            var layeredData = new LayeredData[]
            {
                new LayeredData()
                {
                    RevenueLayers = _settings.AdsAnalyticsSettings.RevenueLayers,
                    Postfix = "",
                    Callback = this.LogLayeredFirebase,
                },
#if TEL_META
                new LayeredData()
                {
                    RevenueLayers = _settings.AdsAnalyticsSettings.MetaRevenueLayers,
                    Postfix = "Meta",
                    Callback = this.LogLayeredFacebook,
                }
#endif
            };

            layeredData.ForEach(data => data.Initialize(_progressService));

            return layeredData;
        }

        private void OnCounterUpdate(AdFormat format, double usdRevenue)
        {
            var revenue = _progressService.UpdateAdCounterRevenue(usdRevenue);

            if (_settings.AdsAnalyticsSettings.AddBanners || AdConstants.IsRewardedOrInterstitial(format))
            {
                var counter = _progressService.GetAdCounter() + 1;

                if (counter >= _settings.AdsAnalyticsSettings.CounterThreshold)
                {
                    var parameters = new[] {
                        new Parameter(FirebaseAnalytics.ParameterValue, revenue),
                        new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
                    };
                    FirebaseService.LogEvent("ad_counter", parameters);

                    _progressService.ClearAdCounterRevenue();
                    counter = 0;
                }

                _progressService.SetAdCounter(counter);
            }
        }
    }

    public enum AdsAnalyticsType
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

    public class LayeredData
    {
        public bool IsRevenueEnough = false;
        public float CurrentRevenue = 0.0f;
        public List<RevenueLayer> RevenueLayers;
        public string Postfix;
        public System.Action<string, double> Callback;

        public void Initialize(IProgressService progressService)
        {
            CurrentRevenue = progressService.GetAdRevenueLayered(Postfix);
            CheckIsRevenueEnough();
        }

        public void CheckIsRevenueEnough()
        {
            if (CurrentRevenue >= RevenueLayers.LastOrDefault().Value)
            {
                IsRevenueEnough = true;
            }
        }
    }
}