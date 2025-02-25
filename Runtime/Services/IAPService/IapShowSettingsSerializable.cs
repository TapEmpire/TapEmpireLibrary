using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services
{
    public class IapShowSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private IapShowSettings _iapShowSettings = null;

        public class IapShowRemoteModel
        {
            public bool Enable = true;
            public List<int> Levels = new();

            public IapShowRemoteModel()
            {
            }

            public IapShowRemoteModel(IapShowSettings settings)
            {
                Enable = settings.Enable;
                Levels = settings.Levels;
            }
        }

        public string TokenName => "IapShowSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<IapShowRemoteModel>();
            _iapShowSettings.Enable = model.Enable;
            _iapShowSettings.Levels = model.Levels;
        }

        public string SerializeJson()
        {
            var model = new IapShowRemoteModel(_iapShowSettings);
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