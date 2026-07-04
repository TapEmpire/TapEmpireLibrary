using System;
using R3;

namespace TapEmpire.Services.Shop
{
    public interface IShopService : IService
    {
        Subject<string> OnShopShown { get; }
        Observable<Unit> OnShopChanged { get; }
        ReadOnlyReactiveProperty<bool> AreFreeItemsAvailable { get; }
        ReadOnlyReactiveProperty<(OfferData Data, DateTime TimeStamp)> ActiveOffer { get; }

        ShopSettings ShopSettings { get; }

        void ShowShop(string placement, bool hasCloseButton = true, bool hasBottomOffset = false, Action onSettingsPressed = null);

        void SetTimeStamp(string key);
        (bool, TimeSpan) HasTimeStampToday(string key);

        // Debug
        void ResetTimers();
        void RewindTimers();
    }
}
