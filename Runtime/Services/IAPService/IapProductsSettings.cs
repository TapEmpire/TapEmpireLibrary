using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/IapProductsSettings", fileName = "IapProductsSettings")]
    public class IapProductsSettings : ScriptableObject
    {
        public bool HasVerification = true;

        [SerializeField] private List<IapOffer> _products = new();

        public List<IapOffer> Products
        {
            get => _products;
            set => _products = value;
        }
    }
}