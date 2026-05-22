using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using TapEmpire.Services;

namespace TapEmpire.Experimental
{
    internal abstract class AdsSignalsModuleBase : IDisposable
    {
        protected readonly CompositeDisposable _disposables = new();
        protected readonly IProgressService _progressService;
        protected readonly AdsAnalyticsSettings _analyticsSettings;

        private readonly BatchedData _batched;
        private readonly LayeredData _layered;

        protected AdsSignalsModuleBase(IAdsService adsService, IProgressService progressService)
        {
            _progressService = progressService;
            _analyticsSettings = adsService.Settings.AdsAnalyticsSettings;

            _batched = InitializeBatchedData();
            _layered = InitializeLayeredData();

            _batched.Initialize(progressService);
            _layered.Initialize(progressService);

            adsService.OnImpression.Subscribe(OnImpression).AddTo(_disposables);
        }

        public virtual void Dispose() => _disposables.Dispose();

        protected abstract BatchedData InitializeBatchedData();
        protected abstract LayeredData InitializeLayeredData();

        protected abstract void LogBatched(double revenue);
        protected abstract void LogLayered(string name, double revenue);

        protected virtual void OnImpression(AdImpressionData data)
        {
            if (data.Currency == "USD")
            {
                ProcessBatched(data.Revenue);
                ProcessLayered(data.Revenue);
            }
        }

        protected void ProcessBatched(double price)
        {
            switch (_batched.BatchType)
            {
                case BatchType.Taichi:
                    UpdateBatched(price);
                    break;
                case BatchType.Once:
                    if (UpdateBatched(price))
                    {
                        _progressService.SetOnceBatched(_batched.Postfix);
                        _batched.IsBatchedOnce = true;
                    }
                    break;
            }
        }

        private bool UpdateBatched(double price)
        {
            _batched.Revenue += price;

            if (_batched.Revenue >= _batched.Threshold || _batched.IsBatchedOnce)
            {
                LogBatched(_batched.Revenue);
                _progressService.ClearAdRevenueBatched(_batched.Postfix);
                _batched.Revenue = 0.0;
                return true;
            }
            else
            {
                _progressService.SetAdRevenueBatched(_batched.Revenue, _batched.Postfix);
                return false;
            }
        }

        protected void ProcessLayered(double price)
        {
            if (_layered.IsRevenueEnough) return;

            var newRevenue = _progressService.UpdateAdRevenueLayered(price, _layered.Postfix);

            foreach (var layer in _layered.RevenueLayers)
            {
                if (layer.Value > newRevenue)
                    break;

                if (layer.Value > _layered.CurrentRevenue)
                {
                    LogLayered(layer.Name, newRevenue);
                }
            }

            _layered.CurrentRevenue = newRevenue;
            _layered.CheckIsRevenueEnough();
        }
    }

    internal class BatchedData
    {
        public double Revenue;
        public bool IsBatchedOnce;
        public BatchType BatchType;
        public double Threshold;
        public string Postfix;

        public void Initialize(IProgressService progressService)
        {
            Revenue = progressService.GetAdRevenueBatched(Postfix);
            IsBatchedOnce = BatchType == BatchType.Once && progressService.GetOnceBatched(Postfix);
        }
    }

    internal class LayeredData
    {
        public bool IsRevenueEnough;
        public float CurrentRevenue;
        public List<RevenueLayer> RevenueLayers;
        public string Postfix;

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
