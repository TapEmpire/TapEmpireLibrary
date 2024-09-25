using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;
using UnityEngine;
using UnityEngine.Video;

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
            if (eventName.StartsWith("ADS") || eventName.StartsWith("SESSION"))
            {
                return;
            }

            if (eventName == "Level_started")
            {
                var cycle = eventParams["cycle"];
                var level = eventParams["level"];
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"cycle_{cycle}", $"level_{level}");
                return;
            }

            if (eventName == "Level_completed")
            {
                var cycle = eventParams["cycle"];
                var level = eventParams["level"];
                var reason = eventParams["reason"].ToString();
                var status = reason == "win" ? GAProgressionStatus.Complete :
                             reason == "lost" ? GAProgressionStatus.Fail : GAProgressionStatus.Undefined;
                var layer = eventParams.TryGetValue("layer", out var value) ? (int)value : 0;
                if (status != GAProgressionStatus.Undefined)
                {
                    GameAnalytics.NewProgressionEvent(status, $"cycle_{cycle}", $"level_{level}", $"layer_{layer}");
                }
                return;
            }

            GameAnalytics.NewDesignEvent(eventName, eventParams);
        }

        public void LogEvent(string eventName, int value)
        {
            GameAnalytics.NewDesignEvent(eventName, value);
        }

        public void SetUserProperty(string propertyName, int value)
        {
            // GameAnalytics.SetGlobalCustomEventFields( new Dictionary<string, object>() { { propertyName, value} });
        }

        public void SetUserProperty(string propertyName, string value)
        {
            // GameAnalytics.SetGlobalCustomEventFields( new Dictionary<string, object>() { { propertyName, value} });
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            if (properties.TryGetValue("RemoteConfig", out var remoteConfig))
            {
                GameAnalytics.SetCustomDimension02(remoteConfig.ToString());
            }
            // GameAnalytics.SetGlobalCustomEventFields(properties);
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