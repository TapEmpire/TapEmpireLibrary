using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    public class ResourcesSettings<ResourceType> : ScriptableObject
    {
        public List<ResourceSettings<ResourceType>> Resources;

        public ResourceSettings<ResourceType> GetSettings(ResourceType type)
        {
            return Resources.Find(setting => EqualityComparer<ResourceType>.Default.Equals(setting.ResourceType, type));
        }

        public Sprite GetFlyingSprite(ResourceType type)
        {
            return GetSettings(type).FlyingSprite;
        }
    }

    [Serializable]
    public struct ResourceSettings<ResourceT>
    {
        public ResourceT ResourceType;
        public int MaxAmount;
        public int ReplenishTime;
        public int InitialAmount;
        public Sprite FlyingSprite;
    }
}