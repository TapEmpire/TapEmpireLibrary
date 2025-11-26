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
        [SerializeField] private OfferSettings _settings;

        public OfferSettings Settings => _settings;

        // public Observable<Unit> OnShopChanged => _onShopChanged;
        // public ReadOnlyReactiveProperty<bool> AreFreeItemsAvailable => _areFreeItemsAvailable;
        // public ReadOnlyReactiveProperty<(OfferData, DateTime)> ActiveOffer => _activeOffer;

        // private DiContainer _diContainer;
        // private IProgressService _progressService;
        // private IIapService _iapService;

        // private Subject<Unit> _onShopChanged = new();
        // private ReactiveProperty<(OfferData Data, DateTime TimeStamp)> _activeOffer = new();
        // private ReactiveProperty<bool> _areFreeItemsAvailable = new(false);
        // private CancellableTask _offerTimer = null;
        // private CancellableTask _midnightTimer = null;

        // private List<string> _freeItemsKeys = new();
        // private CompositeDisposable _disposables = new();

        // [Inject]
        // private void Construct(IProgressService progressService, IIapService iapService, DiContainer diContainer)
        // {
        //     _diContainer = diContainer;
        //     _progressService = progressService;
        //     _iapService = iapService;
        // }

        // protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        // {
        //     _freeItemsKeys = _shopSettings.Sections
        //         .OfType<CommonSectionData>()
        //         .SelectMany(section => section.Products)
        //         .Where(product => product.Type == ProductType.Free || product.Type == ProductType.Ads)
        //         .Select(product => product.Key)
        //         .ToList();

        //     UpdateCurrentOffer();
        //     SetMidnightTimer();

        //     _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);
        //     return base.OnInitializeAsync(cancellationToken);
        // }

        // protected override void OnRelease()
        // {
        //     _offerTimer?.Dispose();
        //     _midnightTimer?.Dispose();
        //     _disposables.Dispose();
        //     base.OnRelease();
        // }

        public OfferRuntimeData GetOffer(string placement)
        {
            return null;
        }

        public OfferRuntimeData GetOffer(OfferType type, Rarity rarity)
        {
            return null;
        }

        private void OnPurchaseSuccess(string productId)
        {
            
        }
    }
}
