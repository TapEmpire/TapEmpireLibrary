using System;
using System.Collections.Generic;
using TapEmpire.Services;
using TapEmpire.Services.Offer;
using Zenject;

namespace WordGame.Services.Offer
{
    [Serializable]
    public class LevelCompleteCondition : ICondition
    {
        public List<int> Levels;
    }

    [Serializable]
    public class LevelCompleteConditionHandler : ConditionHandler<LevelCompleteCondition>
    {
        private IProgressService _progressService;

        public override void Initialize(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
        }

        public override bool Handle(LevelCompleteCondition condition)
        {
            var level = _progressService.GetLevelProgress();
            return condition.Levels.Contains(level);
        }
    }
}