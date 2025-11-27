using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Offer
{
    public class OfferService : Initializable, IOfferService
    {
        [field: SerializeField] public OfferSettings Settings { get; private set; }

        private Rarity _currentRarity = Rarity.Five;

        private DiContainer _diContainer;
        private IProgressService _progressService;
        private IIapService _iapService;

        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IProgressService progressService, IIapService iapService, DiContainer diContainer)
        {
            _diContainer = diContainer;
            _progressService = progressService;
            _iapService = iapService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _currentRarity = _progressService.GetRarity();

            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);
            return base.OnInitializeAsync(cancellationToken);
        }

        // protected override void OnRelease()
        // {
        //     _offerTimer?.Dispose();
        //     _midnightTimer?.Dispose();
        //     _disposables.Dispose();
        //     base.OnRelease();
        // }

        public (BaseOfferUIView, OfferRuntimeData) GetOffer(string placement)
        {
            var offerType = Settings.Placements[placement].First();
            var offerData = Settings.Offers[offerType];

            return (offerData.Element, offerData.ToRuntime(_currentRarity));
        }

        public (BaseOfferUIView, OfferRuntimeData) GetOffer(OfferType type, Rarity rarity)
        {
            var offerData = Settings.Offers[type];
            return (offerData.Element, offerData.ToRuntime(rarity));
        }

        private void OnPurchaseSuccess(string productId)
        {
            var rarity = _iapService.GetOfferInfoById(productId).Rarity;
            _currentRarity = MathUtility.Max(_currentRarity, rarity);
            _progressService.SetRarity(_currentRarity);
        }
    }
}
