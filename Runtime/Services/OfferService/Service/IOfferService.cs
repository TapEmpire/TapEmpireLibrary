using System;
using R3;

namespace TapEmpire.Services.Offer
{
    public interface IOfferService : IService
    {
        OfferSettings Settings { get; }

        OfferRuntimeData GetOffer(string placement);
        OfferRuntimeData GetOffer(OfferType type, Rarity rarity);

        // Observable<Unit> OnShopChanged { get; }
        // ReadOnlyReactiveProperty<bool> AreFreeItemsAvailable { get; }
        // ReadOnlyReactiveProperty<(OfferData Data, DateTime TimeStamp)> ActiveOffer { get; }

        // void SetTimeStamp(string key);
        // (bool, TimeSpan) HasTimeStampToday(string key);

        // // Debug
        // void ResetTimers();
        // void RewindTimers();
    }
}
