using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    public class ResourcesSettings<ResourceType> : ScriptableObject
    {
        public List<ResourceSettings<ResourceType>> Resources;
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