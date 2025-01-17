
using System;
using System.Collections.Generic;
using R3;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    public interface IPurchasingModule : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsReady { get; }

        public Observable<Product> OnPurchaseSuccess { get; }
        public Observable<string> OnPurchaseRestored { get; }

        public Observable<string> OnPurchaseInProgress { get; }
        public Observable<bool> OnRestoreInProgress { get; }
        public Observable<PurchaseFailArgs> OnProductPurchaseFailed { get; }
        public Observable<InitializationFailureReason> OnInitializationFailed { get; }

        public void BuyProduct(IapSettings product);
        public void BuyProduct(string productId);
        public Product GetProductDetail(string productId);

        public void RestorePurchases();
        public void Init(IReadOnlyCollection<IapSettings> iapCollection);
    }
}