using System;
using R3;

namespace TapEmpire.Services.Shop
{
    public interface IShopService : IService
    {
        ShopSettings ShopSettings { get; }

        Observable<Unit> OnShopChanged { get; }
        ReadOnlyReactiveProperty<bool> AreFreeItemsAvailable { get; }
        ReadOnlyReactiveProperty<(OfferData Data, DateTime TimeStamp)> ActiveOffer { get; }

        void SetTimeStamp(string key);
        (bool, TimeSpan) HasTimeStampToday(string key);

        // Debug
        void ResetTimers();
        void RewindTimers();
    }
}
