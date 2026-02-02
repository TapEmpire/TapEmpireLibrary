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
            public int CounterThreshold = 20;
            public bool AddBanners = false;
            public bool AddBannerRevenue = true;
            public bool EnableMeta = false;
            public BatchType BatchTypeMeta;
            public double ThresholdMeta;
            public bool EnableMetaPurchases = false;
            public bool AddMetaIapBatched = false;
            public bool AddMetaIapLayered = false;
            public List<RevenueLayer> RevenueLayers = null;
            public List<RevenueLayer> MetaRevenueLayers = null;

            public AdsAnalyticsRemoteModel() { }

            public AdsAnalyticsRemoteModel(AdsAnalyticsSettings settings)
            {
                BatchType = settings.BatchType;
                Threshold = settings.Threshold;
                CounterThreshold = settings.CounterThreshold;
                AddBanners = settings.AddBanners;
                AddBannerRevenue = settings.AddBannerRevenue;
                EnableMeta = settings.EnableMeta;
                BatchTypeMeta = settings.BatchTypeMeta;
                ThresholdMeta = settings.ThresholdMeta;
                EnableMetaPurchases = settings.EnableMetaPurchases;
                AddMetaIapBatched = settings.AddMetaIapBatched;
                AddMetaIapLayered = settings.AddMetaIapLayered;
                RevenueLayers = settings.RevenueLayers;
                MetaRevenueLayers = settings.MetaRevenueLayers;
            }

            public void ToSettings(AdsAnalyticsSettings settings)
            {
                settings.BatchType = BatchType;
                settings.Threshold = Threshold;
                settings.CounterThreshold = CounterThreshold;
                settings.AddBanners = AddBanners;
                settings.AddBannerRevenue = AddBannerRevenue;
                settings.EnableMeta = EnableMeta;
                settings.BatchTypeMeta = BatchTypeMeta;
                settings.ThresholdMeta = ThresholdMeta;
                settings.EnableMetaPurchases = EnableMetaPurchases;
                settings.AddMetaIapBatched = AddMetaIapBatched;
                settings.AddMetaIapLayered = AddMetaIapLayered;

                if (RevenueLayers != null)
                {
                    settings.RevenueLayers = RevenueLayers.ToList();
                }

                if (MetaRevenueLayers != null)
                {
                    settings.MetaRevenueLayers = MetaRevenueLayers.ToList();
                }
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
