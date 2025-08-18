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
    public class ResourcesSettingsSerializable<ResourceType> : IRemoteSerializable
    {
        [SerializeField] private ResourcesSettings<ResourceType> _resourcesSettings;

        public class ResourcesSettingsRemoteModel
        {
            public List<ResourceSettings<ResourceType>> Resources;

            public ResourcesSettingsRemoteModel()
            {
            }

            public ResourcesSettingsRemoteModel(ResourcesSettings<ResourceType> resourcesSettings)
            {
                Resources = resourcesSettings.Resources.ToList();
            }
        }

        public string TokenName => "ResourcesSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<ResourcesSettingsRemoteModel>();

            _resourcesSettings.Resources = model.Resources.ToList();
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