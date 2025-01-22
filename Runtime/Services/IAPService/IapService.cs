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
        
        [SerializeReference] 
        private IIapSettingsDecorator[] _iapSettingsDecorators = null;
        
        private IPurchasingModule _purchasingModule;
        private IAdsService _adsService;
        private IapAnalyticsModule _iapAnalyticsModule;
        
        private List<IIapHandler<PackIapSettings>> _purchaseSuccessHandlers = new();
        private List<IIapHandler<PackIapSettings>> _purchaseRestoredHandlers = new();
        
        private ReactiveCommand<string> _onPurchaseSuccess = new();
        private ReactiveCommand<string> _onPurchaseRestored = new();
        private ReactiveCommand<PurchaseFailArgs> _onPurchaseFailed = new();
        private ReactiveCommand<IIapHandler<PackIapSettings>>  _onIapHandle = new ();
        
        public Observable<string> OnPurchaseSuccess => _onPurchaseSuccess;
        public Observable<string> OnPurchaseRestored => _onPurchaseRestored;
        public Observable<PurchaseFailArgs> OnPurchaseFailed => _onPurchaseFailed;
        public Observable<IIapHandler<PackIapSettings>> OnIapHandle => _onIapHandle;
        
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
        
        public PackIapSettings GetPackInfo(string key)
        {
            return _iapSettings.Iaps.FirstOrDefault(x => x.Key == key);
        }

        public void RestoreProducts()
        {
            _purchasingModule.RestorePurchases();
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var iapCollection = ProcessWithDecorators(_iapSettings.Iaps);
            IapSettings = iapCollection.ToDictionary(x => x.Key, x => x);
            _purchasingModule.Init(iapCollection);
            _purchaseSuccessHandlers.Add(new NoAdsIapHandler(_adsService));
            _purchaseRestoredHandlers.Add(new NoAdsIapHandler(_adsService));
            _iapAnalyticsModule.Initialize();
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
        
        private List<PackIapSettings> ProcessWithDecorators(List<PackIapSettings> iapSettings)
        {
            if (_iapSettingsDecorators == null)
            {
                return iapSettings;
            }
            List<PackIapSettings> result = null;
            foreach (var iapSettingsDecorator in _iapSettingsDecorators)
            {
                result = iapSettingsDecorator.Process(iapSettings);
            }
            return result;
        }
    }
} 