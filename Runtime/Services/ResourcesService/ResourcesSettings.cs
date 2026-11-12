using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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
    public class ResourceSettings<ResourceT>
    {
        public ResourceT ResourceType;
        public ResourceKind Kind = ResourceKind.Usable;
        public int InitialAmount;
        public int MaxAmount;
        [ShowIf("@MaxAmount > 0")] public bool IsMaxStrict;
        public int FreeUntil;
        public int ReplenishTime;
        public Sprite FlyingSprite;
    }

    public enum ResourceKind
    {
        Usable,
        Tickable
    }
}