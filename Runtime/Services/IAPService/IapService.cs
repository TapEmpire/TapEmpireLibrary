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
        [field: SerializeField] public string AdjustPurchaseToken { get; private set; }
        [field: SerializeField] public IapProductsSettings Settings { get; private set; }
        [SerializeField] private IapShowSettings _iapShowSettings;
        [SerializeField] private UIView _noAdsPopupView;
        [SerializeField] private UIView _iapLoadingView;
        [SerializeReference] private IIapHandler[] _iapHandlers;

        public bool IsPayer { get; private set; }

        private readonly Dictionary<Type, IIapHandler> _handlers = new();

        private IPurchasingModule _purchasingModule;
        private IAdsService _adsService;
        private IProgressService _progressService;
        private IUIService _uiService;
        private DiContainer _diContainer;

        private ReactiveCommand<string> _onPurchaseSuccess = new();
        private ReactiveCommand<Product> _onPurchaseSuccessDetailed = new();
        private ReactiveCommand<string> _onPurchaseRestored = new();
        private ReactiveCommand<PurchaseFailArgs> _onPurchaseFailed = new();
        private ReactiveCommand<IIapHandler> _onIapHandle = new();
        private Action _onIapShownCallback;

        private Dictionary<string, IapOffer> _storeOffers = new();
        private List<int> _iapShowProgress = new();

        public Observable<string> OnPurchaseSuccess => _onPurchaseSuccess;
        public Observable<Product> OnPurchaseSuccessDetailed => _onPurchaseSuccessDetailed;
        public Observable<string> OnPurchaseRestored => _onPurchaseRestored;
        public Observable<PurchaseFailArgs> OnPurchaseFailed => _onPurchaseFailed;
        public Observable<IIapHandler> OnIapHandle => _onIapHandle;

        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(DiContainer diContainer, IProgressService progressService, IAdsService adsService, IUIService uiService)
        {
            _adsService = adsService;
            _progressService = progressService;
            _uiService = uiService;
            _diContainer = diContainer;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new();

            _purchasingModule = new UnityPurchasingModule(_progressService);
            _purchasingModule.OnPurchaseSuccess.Subscribe(OnProductPurchaseSuccess).AddTo(_disposables);
            _purchasingModule.OnProductPurchaseFailed.Subscribe(OnProductPurchaseFailed).AddTo(_disposables);
            _purchasingModule.OnPurchaseRestored.Subscribe(OnProductPurchaseRestored).AddTo(_disposables);
            _purchasingModule.IsInitialized.Subscribe(OnPurchasingInitialized).AddTo(_disposables);

            _purchasingModule.OnPurchaseInProgress.Subscribe(OnPurchaseInProgress).AddTo(_disposables);

            var iapCollection = Settings.Products;
            _storeOffers = iapCollection.ToDictionary(x => x.GetStoreID(), x => x);
            _purchasingModule.Init(iapCollection, Settings.HasVerification);
            Array.ForEach(_iapHandlers, handler => InitializeAndRegisterHandler(handler));

            new IapAnalyticsModule(_diContainer).AddTo(_disposables);
            new IapExtraModule(_diContainer).AddTo(_disposables);

            _iapShowProgress = _progressService.GetIapShowProgress();
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
        }

        public void RegisterHandler<T>(IIapHandler<T> handler) where T : IIapProduct
        {
            _handlers[typeof(T)] = handler;
        }

        public T GetHandler<T>() where T : IIapHandler
        {
            return _handlers.Values.OfType<T>().FirstOrDefault();
        }

        private void InitializeAndRegisterHandler(IIapHandler handler)
        {
            handler.Initialize(_diContainer);
            _handlers[handler.GetProductType()] = handler;
        }

        public void BuyProduct(IapOffer iapId)
        {
            _purchasingModule.BuyProduct(iapId);
        }

        public void BuyProduct(string key)
        {
            var offer = Settings.Products.FirstOrDefault(x => x.Key == key);
            if (offer == null)
            {
                Debug.LogError($"can't find offer with key [{key}]!");
                return;
            }

            _purchasingModule.BuyProduct(offer.GetStoreID());
        }

        public Product GetProductInfo(string key)
        {
            var offer = Settings.Products.FirstOrDefault(x => x.Key == key);
            if (offer != null)
                return _purchasingModule.GetProductDetail(offer.GetStoreID());
            Debug.LogError($"can't find offer with key [{key}]!");
            return null;
        }

        private void OnPurchaseInProgress(string purchaseId)
        {
            if (string.IsNullOrEmpty(purchaseId))
            {
                _uiService.TryCloseViewAsync<IapLoadingViewModel>(default).Forget();
            }
            else
            {
                _uiService.OpenViewAsync(_iapLoadingView, new IapLoadingViewModel(), default).Forget();
            }
        }

        public Product GetProductInfoByStoreId(string key)
        {
            return _purchasingModule.GetProductDetail(key);
        }

        public IapOffer GetOfferInfoByStoreId(string key)
        {
            return Settings.Products.FirstOrDefault(x => x.GetStoreID() == key);
        }

        public IapOffer GetOfferInfoById(string key)
        {
            return Settings.Products.FirstOrDefault(x => x.Key == key);
        }

        public void RestoreProducts()
        {
            _purchasingModule.RestorePurchases();
        }

        public void ShowOnLevel(int level, Action onComplete)
        {
            _progressService.TryGetBoolProp(ProgressBoolProp.DisableAds, out var adsDisabled);

            if (!_iapShowSettings.Enable || adsDisabled)
            {
                onComplete.Invoke();
                return;
            }

            var shouldShowIAP = _iapShowSettings.Levels.Exists(targetLevel => targetLevel == level);

            if (shouldShowIAP)
            {
                var wasShown = _iapShowProgress.Contains(level);
                if (!wasShown)
                {
                    _onIapShownCallback = onComplete;
                    _iapShowProgress.Add(level);
                    _progressService.SetIapShowProgress(_iapShowProgress);
                    var noAdsPopupViewModel = new NoAdsPopupViewModel(new JObject(new JProperty("Level", $"Level_{level}")).ToString());
                    _uiService.OnBeforeCloseView += UiServiceOnOnBeforeCloseView;
                    _uiService.OpenViewAsync(_noAdsPopupView, noAdsPopupViewModel, default).Forget();
                }
                else
                {
                    onComplete.Invoke();
                }
            }
            else
            {
                onComplete.Invoke();
            }
        }

        private void UiServiceOnOnBeforeCloseView(IUIViewModel uiViewModel)
        {
            if (uiViewModel.GetType() == typeof(NoAdsPopupViewModel))
            {
                _uiService.OnBeforeCloseView -= UiServiceOnOnBeforeCloseView;
                _onIapShownCallback?.Invoke();
                _onIapShownCallback = null;
            }
        }

        protected void OnProductPurchaseSuccess(Product product)
        {
            var iapId = product.definition.id;
            Debug.Log($"IAP OnProductPurchaseSuccess {iapId}");
            if (!_storeOffers.ContainsKey(iapId))
                return;

            SetIsPayer(true);
            ProcessPurchase(_storeOffers[iapId]).Forget();
            _progressService.AddPurchase();
            _onPurchaseSuccessDetailed.Execute(product);
            _onPurchaseSuccess.Execute(_storeOffers[iapId].Key);
        }

        protected void OnProductPurchaseFailed(PurchaseFailArgs args)
        {
            Debug.LogError($"IAP OnProductPurchaseFailed {args.IapId} {args.Reason}");
            _onPurchaseFailed.Execute(args);
        }

        protected void OnProductPurchaseRestored(string iapId)
        {
            Debug.Log($"IAP OnPurchaseRestored {iapId}");

            if (!_storeOffers.ContainsKey(iapId))
                return;

            SetIsPayer(true);
            ProcessPurchase(_storeOffers[iapId]).Forget();
            _progressService.AddPurchase();
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

        private void OnPurchasingInitialized(bool isInitialized)
        {
            if (isInitialized)
            {
                IsPayer = GetIsPayer();
            }
        }

        private bool GetIsPayer()
        {
            var isPayer = _progressService.GetIsPayer();
            return isPayer ? true : _purchasingModule.HasAnyPurchases();
        }

        public void SetIsPayer(bool isPayer)
        {
            IsPayer = isPayer;
            _progressService.SetIsPayer(IsPayer);
        }
    }
}