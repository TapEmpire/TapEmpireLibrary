using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Patterns.Strategy;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Offer
{
    public class OfferService : Initializable, IOfferService
    {
        [field: SerializeField] public OfferSettings Settings { get; private set; }

        public Subject<(OfferType, bool, string)> OnOfferShown { get; } = new();
        public Subject<OfferType> OnOfferClosed { get; } = new();

        private Rarity _currentRarity = Rarity.Five;

        private DiContainer _diContainer;
        private IProgressService _progressService;
        private IIapService _iapService;
        private IUIService _uiService;

        private OfferAnalyticsModule _analyticsModule;
        private readonly Dictionary<Type, IHandler> _handlers = new();
        private BoxRandomizer<int> _rarityRandomizer = null;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(DiContainer diContainer, IProgressService progressService, IIapService iapService, IUIService uiService)
        {
            _diContainer = diContainer;
            _progressService = progressService;
            _iapService = iapService;
            _uiService = uiService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new();
            _currentRarity = _progressService.GetRarity();
            var save = _progressService.GetRaritySequence();
            _rarityRandomizer = new BoxRandomizer<int>(Settings.RaritySequence, save, false);

            _analyticsModule = new OfferAnalyticsModule(_diContainer);
            _analyticsModule.AddTo(_disposables);

            Settings.ConditionHandlers.ForEach(handler => InitializeAndRegisterHandler(handler));

            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
            base.OnRelease();
        }

        public T GetHandler<T>() where T : IHandler
        {
            return _handlers.Values.OfType<T>().FirstOrDefault();
        }

        public bool ShowOffer(string placement)
        {
            var offerData = FindOffer(placement);
            if (offerData != null)
            {
                var rarity = _currentRarity.Add(_rarityRandomizer.GetRandomElement());
                _progressService.SetRaritySequence(_rarityRandomizer.CurrentElements);
                ShowOfferInternal(offerData, rarity, placement, true);
            }

            return offerData != null;
        }

        public void ShowOffer(OfferType type, Rarity rarity, string placement)
        {
            var offerData = Settings.Offers[type];
            ShowOfferInternal(offerData, rarity, placement, false);
        }

        public (BaseOfferUIView, OfferRuntimeData) GetOffer(OfferType type, Rarity rarity)
        {
            var offerData = Settings.Offers[type];
            return (offerData.Element, offerData.ToRuntime(rarity));
        }

        private void ShowOfferInternal(OfferData data, Rarity rarity, string placement, bool autoshown)
        {
            _progressService.SetPendingOffer(false);
            var model = new OfferViewModel(data.ToRuntime(rarity), placement);
            model.OnViewClosed.Take(1).Subscribe(_ => OnOfferClosed.OnNext(data.Type));
            _uiService.OpenViewAsync(data.Element, model, default).Forget();
            
            OnOfferShown.OnNext((data.Type, autoshown, placement));
        }

        private bool VerifyCondition(ICondition condition)
        {
            var type = condition.GetType();
            if (_handlers.TryGetValue(type, out var handler) && handler.CanHandle(condition))
            {
                return handler.Handle(condition);
            }

            return false;
        }

        private bool VerifyGenericConditions()
        {
            return Settings.GenericConditions.All(condition => VerifyCondition(condition));
        }

        private OfferData FindOffer(string placement)
        {
            if (!VerifyGenericConditions())
            {
                return null;
            }

            var offerTypes = Settings.Placements[placement];

            foreach (var offerType in offerTypes)
            {
                var offerData = Settings.Offers[offerType];
                if (offerData.Conditions.All(VerifyCondition))
                {
                    return offerData;
                }
            }

            return null;
        }

        private void OnPurchaseSuccess(string productId)
        {
            var rarity = _iapService.GetOfferInfoById(productId).Rarity;
            _currentRarity = MathUtility.Max(_currentRarity, rarity);
            _progressService.SetRarity(_currentRarity);
        }

        private void InitializeAndRegisterHandler(IHandler handler)
        {
            handler.Initialize(_diContainer);
            handler.AddTo(_disposables);
            _handlers[handler.GetSubjectType()] = handler;
        }
    }
}
