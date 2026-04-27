using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Services.Analytics;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public enum LiveOpsTimeUnit
    {
        Minutes,
        Days,
    }

    public class LiveOpsAnalyticsModule<T> : IDisposable where T : ILiveOps
    {
        protected readonly T _liveOps;
        protected readonly CompositeDisposable _disposables = new();

        private readonly IAnalyticsService _analyticsService;
        private readonly LiveOpsTimeUnit _timeUnit;

        public LiveOpsAnalyticsModule(DiContainer diContainer, T liveOps, LiveOpsTimeUnit timeUnit)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _liveOps = liveOps;
            _timeUnit = timeUnit;

            liveOps.OnStarted.Subscribe(OnStarted).AddTo(_disposables);
            liveOps.OnStage.Subscribe(OnStage).AddTo(_disposables);
            liveOps.OnFinished.Subscribe(OnFinished).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected virtual void OnStarted(ILiveOps liveOps) => LogStart(liveOps);
        protected virtual void OnStage(ILiveOps liveOps) => LogStage(liveOps);
        protected virtual void OnFinished(ILiveOps liveOps) => LogFinish(liveOps);

        protected void LogStart(ILiveOps liveOps, params JProperty[] extra)
        {
            LogEvent("Start", new JObject(new JProperty("Id", liveOps.Runtime.Id), extra));
        }

        protected void LogStage(ILiveOps liveOps, params JProperty[] extra)
        {
            var completeTime = GetCompleteTime(liveOps);
            LogEvent("Stage", new JObject(
                new JProperty("Id", liveOps.Runtime.Id),
                new JProperty("Stage", liveOps.Runtime.Inner),
                new JProperty("CompleteTime", completeTime),
                extra));
        }

        protected void LogFinish(ILiveOps liveOps, params JProperty[] extra)
        {
            var completeTime = GetCompleteTime(liveOps);
            LogEvent("End", new JObject(
                new JProperty("Id", liveOps.Runtime.Id),
                new JProperty("Stage", liveOps.Runtime.Inner),
                new JProperty("CompleteTime", completeTime),
                extra));
        }

        private int GetCompleteTime(ILiveOps liveOps)
        {
            var elapsed = DateTime.UtcNow - liveOps.Runtime.StartedAt;
            return _timeUnit switch
            {
                LiveOpsTimeUnit.Minutes => (int)elapsed.TotalMinutes,
                LiveOpsTimeUnit.Days => (int)elapsed.TotalDays,
                _ => throw new ArgumentOutOfRangeException(nameof(_timeUnit), _timeUnit, null),
            };
        }

        private void LogEvent(string eventType, JObject properties)
        {
            _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>
            {
                { "LiveOps", new JObject(new JProperty(_liveOps.Name, new JObject(new JProperty(eventType, properties)))) }
            });
        }
    }
}
