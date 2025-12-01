using System;
using System.Collections.Generic;
using R3;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services.Offer
{
    [Serializable]
    public class AmountCondition : ICondition
    {
        public OfferType OfferType;
        public int Amount = 1;
    }

    [Serializable]
    public class AmountConditionHandler : ConditionHandler<AmountCondition>
    {
        private Dictionary<OfferType, int> _offerAmount = EnumUtility.CreateDefaultDictionary<OfferType, int>(0);
        private CompositeDisposable _disposables = new();

        public override void Initialize(DiContainer diContainer)
        {
            var systemService = diContainer.Resolve<ISystemService>();
            var offerService = diContainer.Resolve<IOfferService>();
            systemService.OnSessionStarted.Subscribe(OnSessionStarted).AddTo(_disposables);
            offerService.OnOfferShown.Subscribe(OnOfferShown).AddTo(_disposables);
        }

        public override bool Handle(AmountCondition condition)
        {
            return _offerAmount[condition.OfferType] < condition.Amount;
        }

        public override void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnSessionStarted(Unit _)
        {
            _offerAmount = EnumUtility.CreateDefaultDictionary<OfferType, int>(0);
        }

        private void OnOfferShown((OfferType OfferType, bool Autoshown) data)
        {
            if (data.Autoshown)
            {
                ++_offerAmount[data.OfferType];
            }
        }
    }
}