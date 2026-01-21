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
            public bool EnableUMP = true;
            public bool EnableATT = true;
            public bool EnableMetica = true;
            public bool EnableMeta = true;
            public bool EnableAppOpen = false;
            public bool EnableBanners = true;
            public bool EnableMrec = true;
            public bool ShouldWaitAppOpen = false;
            public float AppOpenWaitTime = 10.0f;
            public List<int> InterstitialAfterLevels = new();
            public int FromLevel = 1;
            public List<TimerData> TimerData = new();

            public AdsRemoteModel() {}

            public AdsRemoteModel(AdsSettings settings)
            {
                EnableUMP = settings.EnableUMP;
                EnableATT = settings.EnableATT;
                EnableMetica = settings.EnableMetica;
                EnableAppOpen = settings.EnableAppOpen;
                EnableBanners = settings.EnableBanners;
                EnableMrec = settings.EnableMrec;
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
            _adsSettings.EnableUMP = model.EnableUMP;
            _adsSettings.EnableATT = model.EnableATT;
            _adsSettings.EnableMetica = model.EnableMetica;
            _adsSettings.EnableAppOpen = model.EnableAppOpen;
            _adsSettings.EnableBanners = model.EnableBanners;
            _adsSettings.EnableMrec = model.EnableMrec;
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
