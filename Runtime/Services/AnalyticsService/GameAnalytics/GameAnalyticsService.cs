using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;
using UnityEngine;

namespace TapEmpire.Services
{
    public class GameAnalyticsService : Initializable, IAnalyticsService
    {
        private GameAnalytics _gameAnalyticsPrefab;

        public GameAnalyticsService(GameAnalytics gameAnalyticsPrefab)
        {
            _gameAnalyticsPrefab = gameAnalyticsPrefab;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            GameObject.Instantiate(_gameAnalyticsPrefab);
            GameAnalytics.Initialize();

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
        }

        public void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            GameAnalytics.NewDesignEvent(eventName, eventParams);
        }

        public void SetUserProperty(string propertyName, int value)
        {
            GameAnalytics.SetGlobalCustomEventFields( new Dictionary<string, object>() { { propertyName, value} });
        }

        public void SetUserProperty(string propertyName, string value)
        {
            GameAnalytics.SetGlobalCustomEventFields( new Dictionary<string, object>() { { propertyName, value} });
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            GameAnalytics.SetGlobalCustomEventFields(properties);
        }

        public void FlushEvents()
        {
        }

        public static void LogEventStatic(string eventName, Dictionary<string, object> details = null)
        {
            GameAnalytics.NewDesignEvent(eventName, details);
        }
    }
}