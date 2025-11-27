using System;
using R3;

namespace TapEmpire.Services.Offer
{
    public interface IOfferService : IService
    {
        OfferSettings Settings { get; }

        (BaseOfferUIView View, OfferRuntimeData Data) GetOffer(string placement);
        (BaseOfferUIView View, OfferRuntimeData Data) GetOffer(OfferType type, Rarity rarity);
    }
}
