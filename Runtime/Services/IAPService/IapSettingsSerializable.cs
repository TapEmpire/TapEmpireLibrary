using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public class IapSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private DefaultIapSettings _iapSettings;

        public class IapRemoteModel
        {
            public List<PackIapSettings> Iaps = new();
            public IapRemoteModel() { }
            
            public IapRemoteModel(DefaultIapSettings settings)
            {
                Iaps = settings.Iaps;
            }
        }
        
        public string TokenName => "IapSettings";
        
        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<IapRemoteModel>();
            _iapSettings.Iaps = model.Iaps;
        }
        
        public string SerializeJson()
        {
            var model = new IapRemoteModel(_iapSettings);
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
        
        [Button("Test Deserialize")]
        private void DeserializeIapRemoteModel(string jsonString)
        {
            var model = JsonConvert.DeserializeObject<IapRemoteModel>(jsonString);
            _iapSettings.Iaps = model.Iaps;
        }
    }
}