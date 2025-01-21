using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    [Serializable]
    public class IapSettings
    {
        [field: SerializeField] public string GameId { get; private set; } = "";
        [field: SerializeField] public string Key { get; private set; } = "";
        [field: SerializeField] public float Price { get; private set; } = 0;
        [field: NonSerialized] public virtual ProductType ProductType { get; private set; }
        [field: SerializeField] public bool Enabled { get; private set; } = true;
    }
}