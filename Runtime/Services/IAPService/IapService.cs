using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class IapService : Initializable, IIapService
    {
        [SerializeField] private IapSettingsSo<PackIapSettings> _iapSettings;

        private IPurchasingModule _purchasingModule;
        private IAdsService _adsService;
        protected IapSettingsSo<PackIapSettings> IAPSettingsSo { get; set; }
        public Dictionary<string,PackIapSettings> IapSettings { get; set; }
        
        private List<IIapHandler<PackIapSettings>> _purchaseSuccessHandlers = new();
        private List<IIapHandler<PackIapSettings>> _purchaseRestoredHandlers = new();
        
        private ReactiveCommand<string> _onPurchaseSuccess = new();
        public Observable<string> OnPurchaseSuccess => _onPurchaseSuccess;
        
        private ReactiveCommand<PurchaseFailureReason> _onPurchaseFailed = new();
        public Observable<PurchaseFailureReason> OnPurchaseFailed => _onPurchaseFailed;
       
        public Product GetProductInfo(string key)
        {
            return _purchasingModule.GetProductDetail(key);
        }

        [Inject]
        private void Construct(IProgressService progressService, IAdsService adsService)
        {
            _adsService = adsService;
            _purchasingModule = new UnityPurchasingModule(progressService);
            _purchasingModule.OnPurchaseSuccess.Subscribe(OnProductPurchaseSuccess);
            _purchasingModule.OnProductPurchaseFailed.Subscribe(OnProductPurchaseFailed);
            _purchasingModule.OnPurchaseRestored.Subscribe(OnPurchaseRestored);
        }
        
        public void BuyProduct(PackIapSettings iapId)
        {
            _purchasingModule.BuyProduct(iapId);
        }
        
        public void BuyProduct(string iapId)
        {
            _purchasingModule.BuyProduct(iapId);
        }
        
        public void RestoreProducts()
        {
            _purchasingModule.RestorePurchases();
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            IAPSettingsSo = _iapSettings;
            IapSettings = IAPSettingsSo.Iaps.ToDictionary(x => x.Key, x => x);
            _purchasingModule.Init(IAPSettingsSo.Iaps);
            _purchaseSuccessHandlers.Add(new NoAdsIapHandler(_adsService));
            _purchaseRestoredHandlers.Add(new NoAdsIapHandler(_adsService));
            return base.OnInitializeAsync(cancellationToken);
        }
        
        protected void OnProductPurchaseSuccess(string iapId)
        {
            Debug.Log($"IAP OnProductPurchaseSuccess {iapId}");
            if (IapSettings.ContainsKey(iapId))
            {
                ProcessPurchase(IapSettings[iapId], _purchaseSuccessHandlers).Forget();
                _onPurchaseSuccess.Execute(iapId);
            }
        }
        
        protected void OnProductPurchaseFailed(PurchaseFailureReason reason)
        {
            Debug.Log($"IAP OnProductPurchaseFailed {reason}");
            _onPurchaseFailed.Execute(reason);
        }

        protected void OnPurchaseRestored(string iapId)
        {
            Debug.Log($"IAP OnPurchaseRestored{iapId}");

            if (IapSettings.ContainsKey(iapId))
            {
                ProcessPurchase(IapSettings[iapId], _purchaseRestoredHandlers).Forget();
            }
        }
        
        protected async UniTask ProcessPurchase(PackIapSettings settings, IReadOnlyList<IIapHandler<PackIapSettings>> handlers)
        {
            foreach (var iapHandler in handlers)
            {
                await iapHandler.Handle(settings);
            }
        }
    }
} 