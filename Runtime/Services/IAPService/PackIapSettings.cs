using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    [Serializable]
    public class PackIapSettings : IapSettings
    {
        [field: SerializeField] public int MoneyAmount { get; set; } = 1000;
        [field: SerializeField] public int GoldAmount { get; set; } = 10;
        [field: SerializeField] public int TicketAmount { get; set; } = 50;
        [field: SerializeField] public bool DisableAd { get; set; } = true;
        public override ProductType ProductType => ProductType.NonConsumable;
    }
}