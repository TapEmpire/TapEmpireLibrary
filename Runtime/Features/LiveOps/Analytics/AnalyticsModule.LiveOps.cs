using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Services.Analytics;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public abstract class LiveOpsAnalyticsModule<T> : IDisposable where T : ILiveOps
    {
        protected readonly T _liveOps;
        protected readonly CompositeDisposable _disposables = new();

        private readonly IAnalyticsService _analyticsService;

        protected LiveOpsAnalyticsModule(DiContainer diContainer, T liveOps)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _liveOps = liveOps;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected void LogStart(string id, params JProperty[] extra)
        {
            var properties = new JObject(new JProperty("Id", id));
            foreach (var property in extra)
                properties.Add(property);
            LogEvent("Start", properties);
        }

        protected void LogStage(string id, int stage, int completeTime, params JProperty[] extra)
        {
            var properties = new JObject(
                new JProperty("Id", id),
                new JProperty("Stage", stage),
                new JProperty("CompleteTime", completeTime));
            foreach (var property in extra)
                properties.Add(property);
            LogEvent("Stage", properties);
        }

        protected void LogFinish(string id, int stage, int completeTime, params JProperty[] extra)
        {
            var properties = new JObject(
                new JProperty("Id", id),
                new JProperty("Stage", stage),
                new JProperty("CompleteTime", completeTime));
            foreach (var property in extra)
                properties.Add(property);
            LogEvent("End", properties);
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
