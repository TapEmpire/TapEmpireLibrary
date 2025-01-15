
using System;
using System.Collections.Generic;
using R3;

namespace TapEmpire.Services
{
    public interface IPurchasingModule : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsReady { get; }

        public Observable<string> OnPurchaseSuccess { get; }
        public Observable<string> OnPurchaseRestored { get; }

        public Observable<string> OnPurchaseInProgress { get; }
        public Observable<bool> OnRestoreInProgress { get; }
        public Observable<PurchaseFailureReason> OnProductPurchaseFailed { get; }
        public Observable<InitializationFailureReason> OnInitializationFailed { get; }

        public void BuyProduct(IapSettings product);
        public void BuyProduct(string productId);

        public void RestorePurchases();
        public void Init(IReadOnlyCollection<IapSettings> iapCollection);
    }
}