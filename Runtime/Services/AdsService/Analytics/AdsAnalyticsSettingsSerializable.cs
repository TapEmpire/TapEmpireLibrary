using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class AdsAnalyticsSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private AdsAnalyticsSettings _settings = null;

        public class AdsAnalyticsRemoteModel
        {
            public BatchType BatchType;
            public double Threshold;
            
            public AdsAnalyticsRemoteModel() { }

            public AdsAnalyticsRemoteModel(AdsAnalyticsSettings settings)
            {
                BatchType = settings.BatchType;
                Threshold = settings.Threshold;
            }
        }

        public string TokenName => "AdsAnalyticsSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<AdsAnalyticsRemoteModel>();
            _settings.BatchType = model.BatchType;
            _settings.Threshold = model.Threshold;
        }

        public string SerializeJson()
        {
            var model = new AdsAnalyticsRemoteModel(_settings);
            var result = JsonConvert.SerializeObject(model);

            return result;
        }

        [Button("Serialize to file")]
        private void SerializeToFile()
        {
            var json = SerializeJson();
            FileUtility.SaveText("Save waves JSON", TokenName, json);
        }

        [Button("Serialize to console")]
        private void SerializeToConsole()
        {
            var json = SerializeJson();
            Debug.Log(json);
        }
    }
}
