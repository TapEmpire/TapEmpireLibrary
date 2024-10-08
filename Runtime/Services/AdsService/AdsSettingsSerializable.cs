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
    public class AdsSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private AdsSettings _adsSettings = null;

        public class AdsRemoteModel
        {
            public bool EnableAppOpen = true;
            public List<int> InterstitialAfterLevels = new();

            public AdsRemoteModel() {}

            public AdsRemoteModel(AdsSettings settings)
            {
                EnableAppOpen = settings.EnableAppOpen;
                InterstitialAfterLevels = settings.InterstitialAfterLevels.ToList();
            }
        }

        public string TokenName => "AdsSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<AdsRemoteModel>();
            _adsSettings.EnableAppOpen = model.EnableAppOpen;
            _adsSettings.InterstitialAfterLevels = model.InterstitialAfterLevels;
        }

        public string SerializeJson()
        {
            var model = new AdsRemoteModel(_adsSettings);
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
