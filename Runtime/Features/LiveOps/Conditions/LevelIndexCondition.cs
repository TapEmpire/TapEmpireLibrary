using System;
using TapEmpire.Services.Offer;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public class LevelIndexCondition : ICondition
    {
        public int MinStartLevelIndex;
    }

    [Serializable]
    public class LevelIndexConditionHandler : ConditionHandler<LevelIndexCondition>
    {
        private IProgressService _progressService;

        public override void Initialize(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
        }

        public override bool Handle(LevelIndexCondition condition)
        {
            var level = _progressService.GetLevelProgress();
            return  level >= condition.MinStartLevelIndex;
        }
    }
}
