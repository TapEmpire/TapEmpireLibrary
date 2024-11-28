using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public class RateMeSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private RateMeSettings _rateMeSettings = null;
        
        public class RateMeRemoteModel
        {
            public bool Enable = true;
            public List<int> Levels = new();
            public RateMeRemoteModel() {}

            public RateMeRemoteModel(RateMeSettings settings)
            {
                Enable = settings.Enable;
                Levels = settings.Levels;
            }
        }
        
        public string TokenName => "RateMeSettings";
        
        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<RateMeRemoteModel>();
            _rateMeSettings.Enable = model.Enable;
            _rateMeSettings.Levels = model.Levels;
        }

        public string SerializeJson()
        {
            var model = new RateMeRemoteModel(_rateMeSettings);
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