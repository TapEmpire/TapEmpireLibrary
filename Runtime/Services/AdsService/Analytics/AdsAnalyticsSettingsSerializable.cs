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
            public bool EnableMeta = false;
            public BatchType BatchTypeMeta;
            public double ThresholdMeta;
            public bool EnableMetaPurchases = false;
            public bool AddMetaPurchases = false;

            public AdsAnalyticsRemoteModel() { }

            public AdsAnalyticsRemoteModel(AdsAnalyticsSettings settings)
            {
                BatchType = settings.BatchType;
                Threshold = settings.Threshold;
                EnableMeta = settings.EnableMeta;
                BatchTypeMeta = settings.BatchTypeMeta;
                ThresholdMeta = settings.ThresholdMeta;
                EnableMetaPurchases = settings.EnableMetaPurchases;
                AddMetaPurchases = settings.AddMetaPurchases;
            }

            public void ToSettings(AdsAnalyticsSettings settings)
            {
                settings.BatchType = BatchType;
                settings.Threshold = Threshold;
                settings.EnableMeta = EnableMeta;
                settings.BatchTypeMeta = BatchTypeMeta;
                settings.ThresholdMeta = ThresholdMeta;
                settings.EnableMetaPurchases = EnableMetaPurchases;
                settings.AddMetaPurchases = AddMetaPurchases;
            }
        }

        public string TokenName => "AdsAnalyticsSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<AdsAnalyticsRemoteModel>();
            model.ToSettings(_settings);
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
