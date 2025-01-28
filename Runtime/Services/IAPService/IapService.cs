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
        [SerializeField] private IapProductsSettings _iapProductsSettings;
        
        private readonly Dictionary<Type, IIapHandler> _handlers = new();
        
        private IPurchasingModule _purchasingModule;
        private IAdsService _adsService;
        private IapAnalyticsModule _iapAnalyticsModule;
        
        private ReactiveCommand<string> _onPurchaseSuccess = new();
        private ReactiveCommand<string> _onPurchaseRestored = new();
        private ReactiveCommand<PurchaseFailArgs> _onPurchaseFailed = new();
        private ReactiveCommand<IIapHandler>  _onIapHandle = new ();
        
        private Dictionary<string, IapOffer> _storeOffers = new();
        
        public Observable<string> OnPurchaseSuccess => _onPurchaseSuccess;
        public Observable<string> OnPurchaseRestored => _onPurchaseRestored;
        public Observable<PurchaseFailArgs> OnPurchaseFailed => _onPurchaseFailed;
        public Observable<IIapHandler> OnIapHandle => _onIapHandle;
        
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

        public void RegisterHandler<T>(IIapHandler<T> handler) where T : IIapProduct
        {
            _handlers[typeof(T)] = handler;
        }

        public void BuyProduct(IapOffer iapId)
        {
            _purchasingModule.BuyProduct(iapId);
        }
        
        public void BuyProduct(string key)
        {
            var offer = _iapProductsSettings.Products.FirstOrDefault(x => x.Key == key);
            if (offer == null)
            {
                Debug.LogError($"can't find offer with key [{key}]!");
                return;
            }
            _purchasingModule.BuyProduct(offer.GetStoreID());
        }
        
        public Product GetProductInfo(string key)
        {
            var offer = _iapProductsSettings.Products.FirstOrDefault(x => x.Key == key);
            if (offer != null) 
                return _purchasingModule.GetProductDetail(offer.GetStoreID());
            Debug.LogError($"can't find offer with key [{key}]!");
            return null;
        }
        
        public IapOffer GetOfferInfo(string storeKey)
        {
            return _iapProductsSettings.Products.FirstOrDefault(x => x.GetStoreID() == storeKey);
        }

        public void RestoreProducts()
        {
            _purchasingModule.RestorePurchases();
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var iapCollection = _iapProductsSettings.Products;
            _storeOffers = iapCollection.ToDictionary(x => x.GetStoreID(), x => x);
            _purchasingModule.Init(iapCollection);
            RegisterHandler(new NoAdsIapHandler(_adsService));
            _iapAnalyticsModule.Initialize();
            return base.OnInitializeAsync(cancellationToken);
        }
        
        protected void OnProductPurchaseSuccess(string iapId)
        {
            Debug.Log($"IAP OnProductPurchaseSuccess {iapId}");
            if (!_storeOffers.ContainsKey(iapId)) 
                return;
            ProcessPurchase(_storeOffers[iapId]).Forget();
            _onPurchaseSuccess.Execute(iapId);
        }
        
        protected void OnProductPurchaseFailed(PurchaseFailArgs args)
        {
            Debug.Log($"IAP OnProductPurchaseFailed {args.IapId} {args.Reason}");
            _onPurchaseFailed.Execute(args);
        }

        protected void OnProductPurchaseRestored(string iapId)
        {
            Debug.Log($"IAP OnPurchaseRestored{iapId}");

            if (!_storeOffers.ContainsKey(iapId)) 
                return;
            ProcessPurchase(_storeOffers[iapId]).Forget();
            _onPurchaseRestored.Execute(iapId);
        }

        private async UniTask ProcessPurchase(IapOffer settings)
        {
            foreach (var iapProduct in settings.Products)
            {
                var productType = iapProduct.GetType();
                if (_handlers.TryGetValue(productType, out var handler) && handler.CanHandle(iapProduct))
                {
                    await handler.Handle(iapProduct);
                    _onIapHandle.Execute(handler);
                }
            }
        }
    }

} 