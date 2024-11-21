using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;

namespace RagDoll.UI
{
    public class RateUsSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private RateUsSettings _rateUsSettings = null;
        
        public class RateUsRemoteModel
        {
            public bool Enable = true;
            public List<int> RateUsLevels = new();
            public RateUsRemoteModel() {}

            public RateUsRemoteModel(RateUsSettings settings)
            {
                Enable = settings.DefaultNeedRateUs;
                RateUsLevels = settings.Levels;
            }
        }
        
        public string TokenName => "RateUsSettings";
        
        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<RateUsRemoteModel>();
            _rateUsSettings.DefaultNeedRateUs = model.Enable;
            _rateUsSettings.Levels = model.RateUsLevels;
        }

        public string SerializeJson()
        {
            var model = new RateUsRemoteModel(_rateUsSettings);
            var result = JsonConvert.SerializeObject(model);

            return result;
        }

        [Button("Serialize to file")]
        private void SerializeToFile()
        {
            var json = SerializeJson();
            FileUtility.SaveText("Save JSON", TokenName, json);
        }

        [Button("Serialize to console")]
        private void SerializeToConsole()
        {
            var json = SerializeJson();
            Debug.Log(json);
        }
    }
}