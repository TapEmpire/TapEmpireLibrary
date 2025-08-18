using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class AddResourceProduct<ResourceTypeT> : IIapProduct
    {
        [field:SerializeField] public string ProductId { get; private set; } = "AddResource";
        
        public ResourceTypeT ResourceType;
        public int Amount;
    }
}