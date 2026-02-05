using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Services;
using UnityEngine;

namespace TapEmpire.Modules
{
    [Serializable]
    public class SegmentationSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private SegmentationSettings _settings;

        public class SegmentationSettingsRemoteModel
        {
            public List<CampaignSettings> Campaigns = null;

            public SegmentationSettingsRemoteModel() { }

            public SegmentationSettingsRemoteModel(SegmentationSettings settings)
            {
                Campaigns = settings.Campaigns;
            }

            public void ToSettings(SegmentationSettings settings)
            {
                if (Campaigns != null && Campaigns.Count > 0)
                {
                    settings.Campaigns = Campaigns.ToList();
                }
            }
        }

        public string TokenName => "Segmentation";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<SegmentationSettingsRemoteModel>();
            model.ToSettings(_settings);
        }

        public string SerializeJson()
        {
            var model = new SegmentationSettingsRemoteModel(_settings);
            var result = JsonConvert.SerializeObject(model);

            return result;
        }

        [Button("Serialize to console")]
        private void SerializeToConsole()
        {
            var json = SerializeJson();
            Debug.Log(json);
        }
    }
}