using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace TapEmpire.Services
{
    public class UnityPurchasingModule : Initializable, IPurchasingModule
    {
        // private static bool IsGooglePlayStore =>
        //     Application.platform == RuntimePlatform.Android &&
        //     StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay;

        // private static bool IsAppleStore =>
        //     Application.platform == RuntimePlatform.IPhonePlayer ||
        //     Application.platform == RuntimePlatform.OSXPlayer;

        private readonly ReactiveProperty<bool> _isReady = new();

        public ReadOnlyReactiveProperty<bool> IsReady => _isReady;

        private readonly ReactiveProperty<bool> _isInitialized = new();
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;

        private readonly ReactiveProperty<InitializationFailureReason> _onInitializationFailed = new();
        public Observable<InitializationFailureReason> OnInitializationFailed => _onInitializationFailed;

        private readonly ReactiveCommand<Product> _onPurchaseSuccess = new();
        public Observable<Product> OnPurchaseSuccess => _onPurchaseSuccess;

        private readonly ReactiveCommand<string> _onPurchaseRestored = new();
        public Observable<string> OnPurchaseRestored => _onPurchaseRestored;

        private readonly ReactiveCommand<PurchaseFailArgs> _onPurchaseFailed = new();
        public Observable<PurchaseFailArgs> OnProductPurchaseFailed => _onPurchaseFailed;

        private readonly ReactiveProperty<string> _purchaseInProgress = new(string.Empty);
        public Observable<string> OnPurchaseInProgress => _purchaseInProgress;

        private readonly ReactiveProperty<bool> _restoreInProgress = new();
        public Observable<bool> OnRestoreInProgress => _restoreInProgress;

        private readonly ReactiveProperty<Unit> _onDispose = new();
        public Observable<Unit> OnDispose => _onDispose;
        private readonly CompositeDisposable _disposables = new();

        private StoreController _storeController;

        private const string Environment = "production";

        private List<string> _restoredProducts = new();
        private HashSet<string> _grantedOrders = new();
        private HashSet<string> _knownStoreIds = new();

        private readonly IProgressService _progressService;
        private bool _hasVerification = true;

        public UnityPurchasingModule(IProgressService progressService)
        {
            _progressService = progressService;
        }

        public void Init(IReadOnlyCollection<IapOffer> iapSettings, bool hasVerification)
        {
            if (!_progressService.TryLoad(IapDataKeys.RestoredIapKey, out _restoredProducts))
                _restoredProducts = new List<string>();

            _grantedOrders = _progressService.GetPurchaseIds();

            if (_isInitialized.Value)
                return;

            _hasVerification = hasVerification;
            _knownStoreIds = new HashSet<string>(iapSettings.Select(offer => offer.GetStoreID()));

            try
            {
                IsInitialized.Subscribe(_ => UpdateStatus()).AddTo(_disposables);
                OnPurchaseInProgress.Subscribe(_ => UpdateStatus()).AddTo(_disposables);
                OnRestoreInProgress.Subscribe(_ => UpdateStatus()).AddTo(_disposables);
                Initialize(iapSettings).Forget();
            }
            catch (Exception e)
            {
                Debug.LogError($"Iap {e.Message}");
            }
        }

        private async UniTask Initialize(IReadOnlyCollection<IapOffer> iapSettings)
        {
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName(Environment);
                await UnityServices.InitializeAsync(options);
                await InitializeIAP(iapSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Iap {e.Message}");
            }
        }

        private async UniTask InitializeIAP(IReadOnlyCollection<IapOffer> iapSettings)
        {
            _storeController = UnityIAPServices.StoreController();

            _storeController.OnPurchasePending += OnPurchasePending;
            _storeController.OnStoreDisconnected += OnStoreDisconnected;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            _storeController.OnPurchasesFetchFailed += OnPurchasesFetchedFailed;

            await _storeController.Connect();

            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;

            var initialProductsToFetch = iapSettings
                .Select(offer => new ProductDefinition(offer.GetStoreID(), offer.ProductType))
                .ToList();

            _storeController.FetchProducts(initialProductsToFetch);
        }

        private void OnProductsFetched(List<Product> products)
        {
            _storeController.FetchPurchases();
        }

        private void OnPurchasesFetched(Orders orders)
        {
            _isInitialized.Value = true;

            if (_restoreInProgress.Value)
            {
                RestoreOrders(orders);
                _restoreInProgress.Value = false;
            }
        }

        private void RestoreOrders(Orders orders)
        {
            foreach (var order in orders.ConfirmedOrders)
            {
                if (_grantedOrders.Contains(GetUniqueKey(order)))
                    continue;

                var productId = order.CartOrdered.Items().FirstOrDefault()?.Product?.definition.id;
                if (string.IsNullOrEmpty(productId) || !_knownStoreIds.Contains(productId))
                {
                    Debug.Log($"IAP Skip restore of unknown product: {productId}");
                    continue;
                }

                OnPurchasePending(new PendingOrder(order.CartOrdered, order.Info), true);
            }
        }

        public void BuyProduct(IapOffer product)
        {
            BuyProduct(product.GetStoreID());
        }

        public void BuyProduct(string productId)
        {
            if (IsReady.CurrentValue)
            {
                _purchaseInProgress.Value = productId;

                var product = _storeController.GetProductById(productId);
                Debug.Log($"IAP Trying to find product with Id: '{productId}'");
                if (product is { availableToPurchase: true })
                {
                    Debug.Log($"IAP Purchasing product: '{product.definition.id}'");
                    _storeController.PurchaseProduct(product);
                }
                else
                {
                    Debug.Log(
                        "IAP BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    _purchaseInProgress.Value = string.Empty;
                }
            }
            else
            {
                Debug.Log("IAP BuyProductID FAIL. Not initialized.");
                _purchaseInProgress.Value = string.Empty;
            }
        }

        public void RestorePurchases()
        {
            if (!IsReady.CurrentValue)
            {
                Debug.Log("IAP RestorePurchases FAIL. Not initialized.");
                return;
            }

            Debug.Log("IAP RestorePurchases Started ...");

            _restoreInProgress.Value = true;
            _storeController.RestoreTransactions(OnTransactionsRestored);
        }

        public Product GetProductDetail(string productId)
        {
            return _storeController.GetProductById(productId);
        }

        public bool HasAnyPurchases()
        {
            return _storeController.GetPurchases().Count > 0;
        }

        private void OnTransactionsRestored(bool success, string msg)
        {
            Debug.Log($"IAP Transactions restored. {success.ToString()}");
            // _restoreInProgress.Value = false;

            _storeController.FetchPurchases(); // TODO: can be deleted now. It's fixed in the package.

            if (success)
            {
                MobileToast.Show("IAP Products Restored Successfully", true);
            }
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            _isInitialized.Value = false;
            _onInitializationFailed.OnNext(InitializationFailureReason.StoreDisconnect);
            Debug.LogError($"IAP OnStoreDisconnected {description.Message}");
        }

        private void OnPurchasesFetchedFailed(PurchasesFetchFailureDescription description)
        {
            _isInitialized.Value = false;
            _onInitializationFailed.OnNext(InitializationFailureReason.PurchasesFetchFailed);
            Debug.LogError($"IAP OnPurchasesFetchFailed {description.Message}");
        }

        private void OnProductsFetchFailed(ProductFetchFailed failed)
        {
            _isInitialized.Value = false;
            _onInitializationFailed.OnNext(InitializationFailureReason.ProductsFetchFailed);
            Debug.LogError($"IAP OnPurchasesFetchFailed {failed.FailureReason}");
        }

        private void OnPurchasePending(PendingOrder order)
        {
            OnPurchasePending(order, false);
        }

        private void OnPurchasePending(PendingOrder order, bool isRestore)
        {
            Debug.Log($"IAP ProcessPurchase. Is Restore Purchase: {isRestore}");

            if (!VerifyLocal(order))
            {
                Debug.LogError($"IAP Invalid product (prodID): {order.Info.TransactionID}");
                ProvidePurchase(order, false, isRestore);
                return;
            }

            VerifyRemote(order, isRestore);
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            foreach (var item in order.CartOrdered.Items())
            {
                var args = new PurchaseFailArgs
                {
                    IapId = item.Product.definition.id,
                    Reason = (PurchaseFailureReason)order.FailureReason
                };
                _onPurchaseFailed.Execute(args);
            }

            _purchaseInProgress.Value = String.Empty;
        }

        private void UpdateStatus()
        {
            _isReady.Value = string.IsNullOrEmpty(_purchaseInProgress.Value) && !_restoreInProgress.Value && _isInitialized.Value;
        }

        private string GetUniqueKey(Order order)
        {
            // Prefer platform tokens/transaction IDs when available
            // Here we combine store + order id (stable across sessions for durable/subs)
            var prefix = order.Info.Apple != null ? "Apple" :
                         order.Info.Google != null ? "Google" : "Unknown";
            return $"{prefix}-{order.Info.TransactionID}";
        }

        public void Dispose()
        {
            _disposables.Clear();
            _onDispose.Dispose();
        }

        private bool VerifyLocal(PendingOrder order)
        {
#if UNITY_EDITOR
            return true;
#else
            CrossPlatformValidator validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            try
            {
                var result = validator.Validate(order.Info.Receipt);
                return true;
            }
            catch (IAPSecurityException e)
            {
                Debug.LogWarning($"IAP Invalid receipt: {e.Message}");
                return false;
            }
#endif
        }

        private void VerifyRemote(PendingOrder order, bool isRestore)
        {
            Action<bool> callback = isSuccess =>
            {
                Debug.Log($"IAP verification result: {isSuccess}");
                ThreadDispatcher.Enqueue(() => ProvidePurchase(order, isSuccess, isRestore));
            };

#if !IGNORE_VERIFICATION
            if (!_hasVerification)
            {
                callback.Invoke(true);
                return;
            }
#endif

            var unityReceipt = JsonUtility.FromJson<UnityReceipt>(order.Info.Receipt);

#if UNITY_EDITOR || IGNORE_VERIFICATION
            callback.Invoke(true);
#elif UNITY_ANDROID
            var googleReceiptJson = JsonUtility.FromJson<GooglePlayReceiptJson>(unityReceipt.Payload);
            var googleReceipt = JsonUtility.FromJson<GooglePlayReceiptFixed>(googleReceiptJson.json);
            AttributionService.VerifyAndroidPurchase(googleReceipt.productId, googleReceipt.purchaseToken, callback);
#elif UNITY_IOS
            AttributionService.VerifyApplePurchase(order.Info.TransactionID, order.CartOrdered.Items()[0].Product.definition.id, callback);
#endif
        }

        private void ProvidePurchase(PendingOrder order, bool isSuccess, bool isRestore)
        {
            if (isSuccess && _grantedOrders.Add(GetUniqueKey(order)))
            {
                foreach (var item in order.CartOrdered.Items())
                {
                    if (isRestore)
                    {
                        _onPurchaseRestored.Execute(item.Product.definition.id);
                    }
                    else
                    {
                        _onPurchaseSuccess.Execute(item.Product);
                        _storeController.ConfirmPurchase(order);
                    }
                }

                _purchaseInProgress.Value = string.Empty;
            }
            else
            {
                var reason = isRestore ? "Product restore fail" : "Project validation";
                OnPurchaseFailed(new FailedOrder(order, UnityEngine.Purchasing.PurchaseFailureReason.ValidationFailure, reason));
            }
        }
    }

    [System.Serializable]
    public class UnityReceipt
    {
        // public string Store;
        // public string TransactionID;
        public string Payload; // stringified JSON
    }

    [System.Serializable]
    public class GooglePlayReceiptJson
    {
        public string json;
    }

    [System.Serializable]
    public class GooglePlayReceiptFixed
    {
        public string productId;
        public string purchaseToken;
    }
}