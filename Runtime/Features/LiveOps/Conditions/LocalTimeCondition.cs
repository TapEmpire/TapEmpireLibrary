using System;
using TapEmpire.Services.Offer;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public class LocalTimeCondition : ICondition
    {
        public string StartLocalTime; // "HH:mm" format, e.g. "10:30", "14:00"
    }

    [Serializable]
    public class LocalTimeConditionHandler : ConditionHandler<LocalTimeCondition>
    {
        public override void Initialize(DiContainer diContainer)
        {
        }

        public override bool Handle(LocalTimeCondition condition)
        {
            if (string.IsNullOrEmpty(condition.StartLocalTime))
                return true;

            if (!TimeSpan.TryParse(condition.StartLocalTime, out var startLocalTime))
                return true;

            var now = DateTime.Now.TimeOfDay;
            return now >= startLocalTime;
        }
    }
}
