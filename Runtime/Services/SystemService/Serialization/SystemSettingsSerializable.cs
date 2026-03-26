using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class SystemSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private SystemSettings _settings;

        public class SystemSettingsRemoteModel
        {
            public float SessionInterval = 600.0f;
            public bool PlayOfflineNoAds = true;
            public bool PlayOfflineForPayers = true;
            public bool IsPushServiceEnabled = true;

            public SystemSettingsRemoteModel() { }

            public SystemSettingsRemoteModel(SystemSettings settings)
            {
                SessionInterval = settings.SessionInterval;
                PlayOfflineNoAds = settings.PlayOfflineNoAds;
                PlayOfflineForPayers = settings.PlayOfflineForPayers;
                IsPushServiceEnabled = settings.IsPushServiceEnabled;
            }

            public void ToSettings(SystemSettings settings)
            {
                settings.SessionInterval = SessionInterval;
                settings.PlayOfflineNoAds = PlayOfflineNoAds;
                settings.PlayOfflineForPayers = PlayOfflineForPayers;
                settings.IsPushServiceEnabled = IsPushServiceEnabled;

                settings.BroadcastUpdate();
            }
        }

        public string TokenName => "SystemSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<SystemSettingsRemoteModel>();
            model.ToSettings(_settings);
        }

        public string SerializeJson()
        {
            var model = new SystemSettingsRemoteModel(_settings);
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