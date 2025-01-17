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
        private IapAnalyticsModule _iapAnalyticsModule;
        
        private List<IIapHandler<PackIapSettings>> _purchaseSuccessHandlers = new();
        private List<IIapHandler<PackIapSettings>> _purchaseRestoredHandlers = new();
        
        private ReactiveCommand<Product> _onPurchaseSuccess = new();
        private ReactiveCommand<string> _onPurchaseRestored = new();
        private ReactiveCommand<PurchaseFailArgs> _onPurchaseFailed = new();
        private ReactiveCommand<IIapHandler<PackIapSettings>>  _onIapHandle = new ();
        
        public Observable<Product> OnPurchaseSuccess => _onPurchaseSuccess;
        public Observable<string> OnPurchaseRestored => _onPurchaseRestored;
        public Observable<PurchaseFailArgs> OnPurchaseFailed => _onPurchaseFailed;
        public Observable<IIapHandler<PackIapSettings>> OnIapHandle => _onIapHandle;
        
        protected IapSettingsSo<PackIapSettings> IAPSettingsSo { get; set; }
        public Dictionary<string,PackIapSettings> IapSettings { get; set; }
        
        [Inject]
        private void Construct(DiContainer diContainer, IProgressService progressService, IAdsService adsService)
        {
            _adsService = adsService;
            _purchasingModule = new UnityPurchasingModule(progressService);
            _purchasingModule.OnPurchaseSuccess.Subscribe(OnProductPurchaseSuccess);
            _purchasingModule.OnProductPurchaseFailed.Subscribe(OnProductPurchaseFailed);
            _purchasingModule.OnPurchaseRestored.Subscribe(OnProductPurchaseRestored);

             _iapAnalyticsModule = new IapAnalyticsModule(diContainer);
        }
        
        public void BuyProduct(PackIapSettings iapId)
        {
            _purchasingModule.BuyProduct(iapId);
        }
        
        public void BuyProduct(string iapId)
        {
            _purchasingModule.BuyProduct(iapId);
        }
        
        public Product GetProductInfo(string key)
        {
            return _purchasingModule.GetProductDetail(key);
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
            _iapAnalyticsModule.Initialize();
            return base.OnInitializeAsync(cancellationToken);
        }
        
        protected void OnProductPurchaseSuccess(Product product)
        {
            var iapId = product.definition.id;
            Debug.Log($"IAP OnProductPurchaseSuccess {iapId}");
            if (IapSettings.ContainsKey(iapId))
            {
                ProcessPurchase(IapSettings[iapId], _purchaseSuccessHandlers).Forget();
                _onPurchaseSuccess.Execute(product);
            }
        }
        
        protected void OnProductPurchaseFailed(PurchaseFailArgs args)
        {
            Debug.Log($"IAP OnProductPurchaseFailed {args.IapId} {args.Reason}");
            _onPurchaseFailed.Execute(args);
        }

        protected void OnProductPurchaseRestored(string iapId)
        {
            Debug.Log($"IAP OnPurchaseRestored{iapId}");

            if (IapSettings.ContainsKey(iapId))
            {
                ProcessPurchase(IapSettings[iapId], _purchaseRestoredHandlers).Forget();
                _onPurchaseRestored.Execute(iapId);
            }
        }
        
        protected async UniTask ProcessPurchase(PackIapSettings settings, IReadOnlyList<IIapHandler<PackIapSettings>> handlers)
        {
            foreach (var iapHandler in handlers)
            {
                await iapHandler.Handle(settings);
                _onIapHandle.Execute(iapHandler);
            }
        }
    }
} 