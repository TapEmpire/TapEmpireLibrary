using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

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
                if (product is {availableToPurchase: true})
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
            Debug.Log($"IAP OnInitializeFailed {error}");
        }

        public void OnInitializeFailed(UnityEngine.Purchasing.InitializationFailureReason error, string message)
        {
            _isInitialized.Value = false;
            _onInitializationFailed.OnNext((InitializationFailureReason)error);
            Debug.Log($"IAP OnInitializeFailed {message}");
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

            if (_restoreInProgress.Value)
            {
                _onPurchaseRestored.Execute(id);
            }
            else
            {
                _onPurchaseSuccess.Execute(args.purchasedProduct);
            }

            _purchaseInProgress.Value = string.Empty;
            return PurchaseProcessingResult.Complete;
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
    }
}