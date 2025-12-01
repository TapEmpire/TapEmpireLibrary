using System;
using TapEmpire.Patterns.Strategy;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Offer
{
    [Serializable]
    public class PayerCondition : ICondition
    {
    }

    [Serializable]
    public class PayerConditionHandler : ConditionHandler<PayerCondition>
    {
        private IIapService _iapService;

        public override void Initialize(DiContainer diContainer)
        {
            _iapService = diContainer.Resolve<IIapService>();
        }

        public override bool Handle(PayerCondition condition)
        {
            return _iapService.IsPayer;
        }
    }
}