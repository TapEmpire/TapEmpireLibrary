using System;
using TapEmpire.Services.Offer;
using TapEmpire.Services;
using Zenject;

namespace WordGame.Services.Offer
{

    [Serializable]
    public class NoAdsCondition : ICondition
    {
        public bool ShouldHaveNoAds = true;
    }
    
    [Serializable]
    public class NoAdsConditionHandler : ConditionHandler<NoAdsCondition>
    {
        private IAdsService _adsService;

        public override void Initialize(DiContainer diContainer)
        {
            _adsService = diContainer.Resolve<IAdsService>();
        }

        public override bool Handle(NoAdsCondition condition)
        {
            bool hasNoAds = !_adsService.AdsEnabled.CurrentValue;
            return condition.ShouldHaveNoAds == hasNoAds;
        }
    }
}