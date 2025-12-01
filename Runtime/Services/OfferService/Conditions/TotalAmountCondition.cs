using System;
using R3;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services.Offer
{
    [Serializable]
    public class TotalAmountCondition : ICondition
    {
        public int Amount = 2;
    }

    [Serializable]
    public class TotalAmountConditionHandler : ConditionHandler<TotalAmountCondition>
    {
        public int _counter = 0;
        private CompositeDisposable _disposables = new();

        public override void Initialize(DiContainer diContainer)
        {
            var systemService = diContainer.Resolve<ISystemService>();
            var offerService = diContainer.Resolve<IOfferService>();
            systemService.OnSessionStarted.Subscribe(OnSessionStarted).AddTo(_disposables);
            offerService.OnOfferShown.Subscribe(OnOfferShown).AddTo(_disposables);
        }

        public override bool Handle(TotalAmountCondition condition)
        {
            return _counter < condition.Amount;
        }

        public override void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnSessionStarted(Unit _)
        {
            _counter = 0;
        }

        private void OnOfferShown((OfferType OfferType, bool Autoshown) data)
        {
            if (data.Autoshown)
            {
                ++_counter;
            }
        }
    }
}