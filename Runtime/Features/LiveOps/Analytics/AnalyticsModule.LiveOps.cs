using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Services.Analytics;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsAnalyticsModule<T> : IDisposable where T : ILiveOps
    {
        protected readonly T _liveOps;
        protected readonly CompositeDisposable _disposables = new();

        private readonly IAnalyticsService _analyticsService;

        public LiveOpsAnalyticsModule(DiContainer diContainer, T liveOps)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _liveOps = liveOps;

            liveOps.OnStarted.Subscribe(OnStarted).AddTo(_disposables);
            liveOps.OnStage.Subscribe(OnStage).AddTo(_disposables);
            liveOps.OnFinished.Subscribe(OnFinished).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected virtual void OnStarted(ILiveOps liveOps)
        {
            LogStart(liveOps.StateData.Id);
        }

        protected virtual void OnStage(ILiveOps liveOps)
        {
            var completeTime = (int)(DateTime.UtcNow - liveOps.StateData.StartedAt).TotalMinutes;
            LogStage(liveOps.StateData.Id, liveOps.StateData.Inner, completeTime);
        }

        protected virtual void OnFinished(ILiveOps liveOps)
        {
            var completeTime = (int)(DateTime.UtcNow - liveOps.StateData.StartedAt).TotalMinutes;
            LogFinish(liveOps.StateData.Id, liveOps.StateData.Inner, completeTime);
        }

        protected void LogStart(string id, params JProperty[] extra)
        {
            LogEvent("Start", new JObject(new JProperty("Id", id), extra));
        }

        protected void LogStage(string id, int stage, int completeTime, params JProperty[] extra)
        {
            LogEvent("Stage", new JObject(
                new JProperty("Id", id),
                new JProperty("Stage", stage),
                new JProperty("CompleteTime", completeTime),
                extra));
        }

        protected void LogFinish(string id, int stage, int completeTime, params JProperty[] extra)
        {
            LogEvent("End", new JObject(
                new JProperty("Id", id),
                new JProperty("Stage", stage),
                new JProperty("CompleteTime", completeTime),
                extra));
        }

        protected void LogEvent(string eventType, JObject properties)
        {
            _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>
            {
                { "LiveOps", new JObject(new JProperty(_liveOps.Name, new JObject(new JProperty(eventType, properties)))) }
            });
        }
    }
}
