using R3;

namespace TapEmpire.Services
{
    public interface IIapService : IService
    {
        void BuyProduct(PackIapSettings iapId);
        void BuyProduct(string iapId);
        void RestoreProducts();

        public Observable<string> OnPurchaseSuccess { get; }

        public Observable<PurchaseFailureReason> OnPurchaseFailed { get; }
    }
}