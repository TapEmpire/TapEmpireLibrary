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
    public class ResourceSettingsRemoteModel<ResourceT>
    {
        public ResourceT ResourceType;
        public int? MaxAmount;
        public int? ReplenishTime;
        public int? InitialAmount;

        public void ApplyTo(ResourceSettings<ResourceT> settings)
        {
            if (MaxAmount.HasValue) settings.MaxAmount = MaxAmount.Value;
            if (ReplenishTime.HasValue) settings.ReplenishTime = ReplenishTime.Value;
            if (InitialAmount.HasValue) settings.InitialAmount = InitialAmount.Value;
        }
    }

    [Serializable]
    public class ResourcesSettingsSerializable<ResourceType> : IRemoteSerializable
    {
        [SerializeField] private ResourcesSettings<ResourceType> _resourcesSettings;

        public class ResourcesSettingsRemoteModel
        {
            public List<ResourceSettingsRemoteModel<ResourceType>> Resources;

            public ResourcesSettingsRemoteModel()
            {
            }

            public ResourcesSettingsRemoteModel(ResourcesSettings<ResourceType> resourcesSettings)
            {
                Resources = resourcesSettings.Resources.Select(resource => new ResourceSettingsRemoteModel<ResourceType>
                {
                    ResourceType = resource.ResourceType,
                    MaxAmount = resource.MaxAmount,
                    ReplenishTime = resource.ReplenishTime,
                    InitialAmount = resource.InitialAmount,
                }).ToList();
            }
        }

        public string TokenName => "ResourcesSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<ResourcesSettingsRemoteModel>();

            foreach (var remoteResource in model.Resources)
            {
                var settings = _resourcesSettings.Resources.Find(resource =>
                    EqualityComparer<ResourceType>.Default.Equals(resource.ResourceType, remoteResource.ResourceType));

                if (settings != null)
                {
                    remoteResource.ApplyTo(settings);
                }
            }
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