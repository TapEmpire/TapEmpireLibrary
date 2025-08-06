using System;
using System.Collections.Generic;
using System.Linq;
using com.adjust.sdk;
using Cysharp.Threading.Tasks;
using R3;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace TapEmpire.Services
{
    public class UnityPurchasingModule : Initializable, IPurchasingModule, IDetailedStoreListener
    {
        private static bool IsGooglePlayStore =>
            Application.platform == RuntimePlatform.Android &&
            StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay;

        private static bool IsAppleStore =>
            Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer;

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

        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private const string Environment = "production";

        private List<string> _restoredProducts = new();

        private readonly IProgressService _progressService;

        public UnityPurchasingModule(IProgressService progressService)
        {
            _progressService = progressService;
        }

        public void Init(IReadOnlyCollection<IapOffer> iapSettings)
        {
            if (!_progressService.TryLoad(IapDataKeys.RestoredIapKey, out _restoredProducts))
                _restoredProducts = new List<string>();

            if (_isInitialized.Value)
                return;
            try
            {
                var module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
                module.useFakeStoreAlways = true;
                module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
                var builder = ConfigurationBuilder.Instance(module);

                foreach (var iap in iapSettings)
                {
                    builder.AddProduct(iap.GetStoreID(), iap.ProductType);
                }

                IsInitialized.Subscribe(_ => UpdateStatus()).AddTo(_disposables);
                OnPurchaseInProgress.Subscribe(_ => UpdateStatus()).AddTo(_disposables);
                OnRestoreInProgress.Subscribe(_ => UpdateStatus()).AddTo(_disposables);
                Initialize(builder).Forget();
            }
            catch (Exception e)
            {
                Debug.LogError($"Iap {e.Message}");
            }
        }

        private async UniTask Initialize(ConfigurationBuilder builder)
        {
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName(Environment);
                await UnityServices.InitializeAsync(options);
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception e)
            {
                Debug.LogError($"Iap {e.Message}");
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

                var product = _controller.products.WithID(productId);
                Debug.Log($"IAP Trying to find product with Id: '{productId}'");
                if (product is { availableToPurchase: true })
                {
                    Debug.Log($"IAP Purchasing product: '{product.definition.id}'");
                    _controller.InitiatePurchase(product);
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

            if (_restoredProducts.Any())
            {
                _restoredProducts.ForEach(x => _onPurchaseRestored.Execute(x));
                _restoredProducts.Clear();
                _progressService.Save(IapDataKeys.RestoredIapKey, _restoredProducts);
                return;
            }

            if (IsGooglePlayStore)
            {
                _restoreInProgress.Value = true;
                _extensions.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(OnTransactionsRestored);
            }
            else if (IsAppleStore)
            {
                _restoreInProgress.Value = true;
                _extensions.GetExtension<IAppleExtensions>().RestoreTransactions(OnTransactionsRestored);
            }
            else
            {
                Debug.Log("IAP RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public Product GetProductDetail(string productId)
        {
            return _controller.products.WithID(productId);
        }

        private void OnTransactionsRestored(bool success, string msg)
        {
            Debug.Log($"IAP Transactions restored. {success.ToString()}");
            _restoreInProgress.Value = false;

            if (success)
            {
                MobileToast.Show("IAP Products Restored Successfully", true);
            }
        }

        public void OnInitializeFailed(UnityEngine.Purchasing.InitializationFailureReason error)
        {
            _isInitialized.Value = false;
            _onInitializationFailed.OnNext((InitializationFailureReason)error);
            Debug.LogError($"IAP OnInitializeFailed {error}");
        }

        public void OnInitializeFailed(UnityEngine.Purchasing.InitializationFailureReason error, string message)
        {
            _isInitialized.Value = false;
            _onInitializationFailed.OnNext((InitializationFailureReason)error);
            Debug.LogError($"IAP OnInitializeFailed {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log($"IAP ProcessPurchase. Is Restore Purchase: {_restoreInProgress.Value.ToString()}");

            var id = args.purchasedProduct.definition.id;
            if (_isReady.Value)
            {
                _restoredProducts.Add(id);
                _progressService.Save(IapDataKeys.RestoredIapKey, _restoredProducts);
                return PurchaseProcessingResult.Complete;
            }

            if (!VerifyLocal(args))
            {
                Debug.LogError($"Invalid product (prodID): {args.purchasedProduct.definition.id}");
                return PurchaseProcessingResult.Complete;
            }

            VerifyAdjust(args);
            return PurchaseProcessingResult.Pending;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            _isInitialized.Value = true;
        }

        public void OnPurchaseFailed(Product product, UnityEngine.Purchasing.PurchaseFailureReason failureReason)
        {
            var args = new PurchaseFailArgs
            {
                IapId = product.definition.id,
                Reason = (PurchaseFailureReason)failureReason
            };
            _onPurchaseFailed.Execute(args);
            _purchaseInProgress.Value = String.Empty;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var args = new PurchaseFailArgs
            {
                IapId = product.definition.id,
                Reason = (PurchaseFailureReason)failureDescription.reason
            };
            _onPurchaseFailed.Execute(args);
            _purchaseInProgress.Value = String.Empty;
        }

        private void UpdateStatus()
        {
            _isReady.Value = string.IsNullOrEmpty(_purchaseInProgress.Value) && !_restoreInProgress.Value && _isInitialized.Value;
        }

        public void Dispose()
        {
            _disposables.Clear();
            _onDispose.Dispose();
        }

        private bool VerifyLocal(PurchaseEventArgs args)
        {
#if UNITY_EDITOR
            return true;
#else
            CrossPlatformValidator validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            try
            {
                var result = validator.Validate(args.purchasedProduct.receipt);
                return true;
            }
            catch (IAPSecurityException e)
            {
                Debug.LogWarning($"[IAP] Invalid receipt: {e.Message}");
                return false;
            }
#endif
        }

        private void VerifyAdjust(PurchaseEventArgs args)
        {
            Product product = args.purchasedProduct;

            Action<AdjustPurchaseVerificationInfo> callback = (AdjustPurchaseVerificationInfo result) =>
            {
                Debug.Log($"Adjust verification result: {result}");
                bool isSuccess = result.verificationStatus == "success";

                ThreadDispatcher.Enqueue(() => ProvidePurchase(product, isSuccess));
            };

#if UNITY_EDITOR
            callback.Invoke(new AdjustPurchaseVerificationInfo() { code = 200, message = "Debug", verificationStatus = "success" });
#elif UNITY_ANDROID
            var unityReceipt = JsonUtility.FromJson<UnityReceipt>(product.receipt);
            var googleReceipt = JsonUtility.FromJson<GooglePlayReceipt>(unityReceipt.Payload);

            var adjustPlayStorePurchase = new AdjustPlayStorePurchase(product.definition.id, googleReceipt.purchaseToken);
            Adjust.verifyPlayStorePurchase(adjustPlayStorePurchase, callback);
#elif UNITY_IOS
            var adjustAppStorePurchase = new AdjustAppStorePurchase(product.transactionID, product.definition.id, product.receipt);
            Adjust.verifyAppStorePurchase(adjustAppStorePurchase, callback);
#endif
        }

        private void ProvidePurchase(Product product, bool isSuccess)
        {
            if (isSuccess)
            {
                if (_restoreInProgress.Value)
                {
                    _onPurchaseRestored.Execute(product.definition.id);
                }
                else
                {
                    _onPurchaseSuccess.Execute(product);
                }
            }

            _purchaseInProgress.Value = string.Empty;
            _controller.ConfirmPendingPurchase(product);
        }
    }

    [System.Serializable]
    public class UnityReceipt
    {
        public string Store;
        public string TransactionID;
        public string Payload; // stringified JSON
    }
}