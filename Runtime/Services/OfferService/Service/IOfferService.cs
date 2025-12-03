using System;
using R3;
using TapEmpire.Patterns.Strategy;

namespace TapEmpire.Services.Offer
{
    public interface IOfferService : IService
    {
        Subject<(OfferType OfferType, bool Autoshown)> OnOfferShown { get; }
        Subject<OfferType> OnOfferClosed { get; }

        OfferSettings Settings { get; }

        T GetHandler<T>() where T : IHandler;

        bool ShowOffer(string placement);
        void ShowOffer(OfferType type, Rarity rarity, string placement);

        (BaseOfferUIView View, OfferRuntimeData Data) GetOffer(OfferType type, Rarity rarity); // For debug purposes
    }
}
