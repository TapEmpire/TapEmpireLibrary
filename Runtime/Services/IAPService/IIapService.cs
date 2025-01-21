using R3;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    public interface IIapService : IService
    {
        void BuyProduct(PackIapSettings iapId);
        void BuyProduct(string iapId);
        void RestoreProducts();

        public Observable<string> OnPurchaseSuccess { get; }
        public Observable<string> OnPurchaseRestored { get; }
        public Observable<PurchaseFailArgs> OnPurchaseFailed { get; }
        public Observable<IIapHandler<PackIapSettings>> OnIapHandle { get; }
        Product GetProductInfo(string key);
        PackIapSettings GetPackInfo(string key);
    }
}