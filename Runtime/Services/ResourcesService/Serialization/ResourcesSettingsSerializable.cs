using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class ResourcesSettingsSerializable<ResourceType> : IRemoteSerializable
    {
        [SerializeField] private ResourcesSettings<ResourceType> _resourcesSettings;

        public class ResourcesSettingsRemoteModel
        {
            // public int InitialHints = 3;
            // public int MinAdBreakCategories = 5;
            // public int HintsForDisableAd = 10;

            public ResourcesSettingsRemoteModel()
            {
            }

            public ResourcesSettingsRemoteModel(ResourcesSettings<ResourceType> resourcesSettings)
            {
                // InitialHints = gameplaySettings.InitialHints;
                // MinAdBreakCategories = gameplaySettings.MinAdBreakCategories;
                // HintsForDisableAd = gameplaySettings.HintsForDisableAd;
            }
        }

        public string TokenName => "ResourcesSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<ResourcesSettingsRemoteModel>();

            // _gameplaySettings.InitialHints = model.InitialHints;
            // _gameplaySettings.MinAdBreakCategories = model.MinAdBreakCategories;
            // _gameplaySettings.HintsForDisableAd = model.HintsForDisableAd;
        }

        public string SerializeJson()
        {
            var model = new ResourcesSettingsRemoteModel(_resourcesSettings);
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