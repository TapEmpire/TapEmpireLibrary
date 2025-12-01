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
        [SerializeField] private IapProductsSettings _iapProductsSettings;
        [SerializeField] private IapShowSettings _iapShowSettings;
        [SerializeField] private UIView _noAdsPopupView;
        [SerializeField] private UIView _iapLoadingView;
        [SerializeReference] private IIapHandler[] _iapHandlers;

        public bool IsPayer { get; private set; }

        private readonly Dictionary<Type, IIapHandler> _handlers = new();

        private IPurchasingModule _purchasingModule;
        private IAdsService _adsService;
        private IapAnalyticsModule _iapAnalyticsModule;
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

        private CompositeDisposable _disposable = new CompositeDisposable();

        public Observable<string> OnPurchaseSuccess => _onPurchaseSuccess;
        public Observable<Product> OnPurchaseSuccessDetailed => _onPurchaseSuccessDetailed;
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
            _purchasingModule.OnPurchaseSuccess.Subscribe(OnProductPurchaseSuccess).AddTo(_disposable);
            _purchasingModule.OnProductPurchaseFailed.Subscribe(OnProductPurchaseFailed).AddTo(_disposable);
            _purchasingModule.OnPurchaseRestored.Subscribe(OnProductPurchaseRestored).AddTo(_disposable);

            _purchasingModule.OnPurchaseInProgress.Subscribe(OnPurchaseInProgress).AddTo(_disposable);
            _iapAnalyticsModule = new IapAnalyticsModule(diContainer);
            _diContainer = diContainer;
        }

        public void RegisterHandler<T>(IIapHandler<T> handler) where T : IIapProduct
        {
            _handlers[typeof(T)] = handler;
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
            return _iapProductsSettings.Products.FirstOrDefault(x => x.GetStoreID() == key);
        }

        public IapOffer GetOfferInfoById(string key)
        {
            return _iapProductsSettings.Products.FirstOrDefault(x => x.Key == key);
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
                    _uiService.OpenViewAsync(_noAdsPopupView, noAdsPopupViewModel, CancellationToken.None).Forget();
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

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            IsPayer = GetIsPayer();

            var iapCollection = _iapProductsSettings.Products;
            _storeOffers = iapCollection.ToDictionary(x => x.GetStoreID(), x => x);
            _purchasingModule.Init(iapCollection, _iapProductsSettings.HasVerification);
            Array.ForEach(_iapHandlers, handler => InitializeAndRegisterHandler(handler));

            _iapAnalyticsModule.Initialize();

            _iapShowProgress = _progressService.GetIapShowProgress();
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _disposable.Dispose();
        }

        protected void OnProductPurchaseSuccess(Product product)
        {
            var iapId = product.definition.id;
            Debug.Log($"IAP OnProductPurchaseSuccess {iapId}");
            if (!_storeOffers.ContainsKey(iapId))
                return;

            UpdateIsPayer();
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

            UpdateIsPayer();
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

        private bool GetIsPayer()
        {
            var isPayer = _progressService.GetIsPayer();
            return isPayer ? true : _purchasingModule.HasAnyPurchases();
        }

        private void UpdateIsPayer()
        {
            IsPayer = true;
            _progressService.SetIsPayer(IsPayer);
        }
    }
}