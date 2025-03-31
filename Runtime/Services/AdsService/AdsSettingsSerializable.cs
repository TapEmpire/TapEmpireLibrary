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
            public bool EnableAppOpen = false;
            public bool ShouldWaitAppOpen = false;
            public float AppOpenWaitTime = 10.0f;
            public List<int> InterstitialAfterLevels = new();
            public int FromLevel = 1;
            public List<TimerData> TimerData = new();

            public AdsRemoteModel() {}

            public AdsRemoteModel(AdsSettings settings)
            {
                EnableAppOpen = settings.EnableAppOpen;
                ShouldWaitAppOpen = settings.ShouldWaitAppOpen;
                AppOpenWaitTime = settings.AppOpenWaitTime;
                InterstitialAfterLevels = settings.InterstitialAfterLevels.ToList();
                FromLevel = settings.FromLevel;
                TimerData = settings.TimerData.ToList();
            }
        }

        public string TokenName => "AdsSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<AdsRemoteModel>();
            _adsSettings.EnableAppOpen = model.EnableAppOpen;
            _adsSettings.ShouldWaitAppOpen = model.ShouldWaitAppOpen;
            _adsSettings.AppOpenWaitTime = model.AppOpenWaitTime;
            _adsSettings.InterstitialAfterLevels = model.InterstitialAfterLevels;
            _adsSettings.FromLevel = model.FromLevel;
            _adsSettings.TimerData = model.TimerData;
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
