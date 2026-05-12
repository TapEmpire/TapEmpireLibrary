using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public class LiveOpsSerializable : IRemoteSerializable
    {
        [SerializeField] private LiveOpsSettings _settings;

        public string TokenName => "LiveOps";

        public void DeserializeJson(JToken token)
        {
            var pairs = token.ToObject<Dictionary<string, int>>();
            foreach (var liveOps in _settings.LiveOps)
            {
                if (pairs.TryGetValue(liveOps.Name, out var level))
                {
                    liveOps.MinStartLevelIndex = level;
                }
            }
        }

        public string SerializeJson()
        {
            return JsonConvert.SerializeObject(
                _settings.LiveOps.ToDictionary(
                    liveOps => liveOps.Name,
                    liveOps => liveOps.MinStartLevelIndex));
        }

        [Button("Serialize to console")]
        private void SerializeToConsole() => Debug.Log(SerializeJson());
    }
}
