using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class IapService : Initializable, IIapService
    {
        [SerializeField] private IapProductsSettings _iapProductsSettings;
        [SerializeField] private IapShowSettings _iapShowSettings;
        [SerializeField] private NoAdsPopupView _noAdsPopupView;

        private readonly Dictionary<Type, IIapHandler> _handlers = new();
        
        private IPurchasingModule _purchasingModule;
        private IAdsService _adsService;
        private IapAnalyticsModule _iapAnalyticsModule;
        private IProgressService _progressService;
        private IUIService _uiService;
        
        private ReactiveCommand<string> _onPurchaseSuccess = new();
        private ReactiveCommand<string> _onPurchaseRestored = new();
        private ReactiveCommand<PurchaseFailArgs> _onPurchaseFailed = new();
        private ReactiveCommand<IIapHandler>  _onIapHandle = new ();
        
        private Dictionary<string, IapOffer> _storeOffers = new();
        private List<int> _iapShowProgress = new();

        private CompositeDisposable _disposable = new CompositeDisposable();
        
        public Observable<string> OnPurchaseSuccess => _onPurchaseSuccess;
        public Observable<string> OnPurchaseRestored => _onPurchaseRestored;
        public Observable<PurchaseFailArgs> OnPurchaseFailed => _onPurchaseFailed;
        public Observable<IIapHandler> OnIapHandle => _onIapHandle;
        
        [Inject]
        private void Construct(DiContainer diContainer, IProgressService progressService, IAdsService adsService, IUIService uiService)
        {
            _adsService = adsService;
            _progressService = progressService;
            _uiService = uiService;
            _purchasingModule = new UnityPurchasingModule(progressService);
            _disposable.Add(_purchasingModule.OnPurchaseSuccess.Subscribe(OnProductPurchaseSuccess));
            _disposable.Add(_purchasingModule.OnProductPurchaseFailed.Subscribe(OnProductPurchaseFailed));
            _disposable.Add(_purchasingModule.OnPurchaseRestored.Subscribe(OnProductPurchaseRestored));
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

        public void ShowOnLevel(int level)
        {
            _progressService.TryGetBoolProp(ProgressBoolProp.DisableAds, out var adsDisabled);
            
            if (!_iapShowSettings.Enable || adsDisabled)
            {
                return;
            }

            var shouldShowIAP = _iapShowSettings.Levels.Exists(targetLevel => targetLevel == level);

            if (shouldShowIAP)
            {
                var wasShown = _iapShowProgress.Contains(level);
                if (!wasShown)
                {
                    _iapShowProgress.Add(level);
                    _progressService.SetIapShowProgress(_iapShowProgress);
                    var noAdsPopupViewModel = new NoAdsPopupViewModel(_uiService, this, new JObject(new JProperty("Level", $"Level_{level}")).ToString());
                    _uiService.OpenViewAsync(_noAdsPopupView, noAdsPopupViewModel, CancellationToken.None).Forget();
                }
            }
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var iapCollection = _iapProductsSettings.Products;
            _storeOffers = iapCollection.ToDictionary(x => x.GetStoreID(), x => x);
            _purchasingModule.Init(iapCollection);
            RegisterHandler(new NoAdsIapHandler(_adsService));
            _iapAnalyticsModule.Initialize();

            _iapShowProgress = _progressService.GetIapShowProgress();
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

        protected override void OnRelease()
        {
            _disposable.Dispose();
        }
    }
} 