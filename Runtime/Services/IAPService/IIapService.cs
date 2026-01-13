using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    public interface IIapService : IService
    {
        IapProductsSettings Settings { get; }
        string AdjustPurchaseToken { get; }
        bool IsPayer { get; }

        void RegisterHandler<T>(IIapHandler<T> handler) where T : IIapProduct;
        T GetHandler<T>() where T : IIapHandler;
        void BuyProduct(IapOffer iapId); // Deprecated, not used.
        void BuyProduct(string iapId, string placement = null);
        void RestoreProducts();

        public Observable<string> OnPurchaseSuccess { get; }
        public Observable<(Product, string, string)> OnPurchaseSuccessDetailed { get; }
        public Observable<string> OnPurchaseRestored { get; }
        public Observable<PurchaseFailArgs> OnPurchaseFailed { get; }
        public Observable<IIapHandler> OnIapHandle { get; }
        Product GetProductInfo(string key);
        Product GetProductInfoByStoreId(string key);
        IapOffer GetOfferInfoByStoreId(string key);
        IapOffer GetOfferInfoById(string key);
        void ShowOnLevel(int dataLevelIndex, Action callback);

        // public for debug purposes
        public void SetIsPayer(bool isPayer);
    }
}