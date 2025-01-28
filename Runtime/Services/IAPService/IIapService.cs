using R3;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    public interface IIapService : IService
    {
        void RegisterHandler<T>(IIapHandler<T> handler) where T : IIapProduct;
        void BuyProduct(IapOffer iapId);
        void BuyProduct(string iapId);
        void RestoreProducts();

        public Observable<string> OnPurchaseSuccess { get; }
        public Observable<string> OnPurchaseRestored { get; }
        public Observable<PurchaseFailArgs> OnPurchaseFailed { get; }
        public Observable<IIapHandler> OnIapHandle { get; }
        Product GetProductInfo(string key);
        IapOffer GetOfferInfo(string key);
    }
}