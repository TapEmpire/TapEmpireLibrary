using System;
using R3;

namespace TapEmpire.Services.Offer
{
    public interface IOfferService : IService
    {
        OfferSettings Settings { get; }

        (BaseOfferUIView View, OfferRuntimeData Data) GetOffer(string placement);
        (BaseOfferUIView View, OfferRuntimeData Data) GetOffer(OfferType type, Rarity rarity);

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
