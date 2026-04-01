using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/IapProductsSettings", fileName = "IapProductsSettings")]
    public class IapProductsSettings : ScriptableObject
    {
        public bool HasVerification = true;
        public bool DisableAdsForPayers = true;

        [SerializeField] private List<IapOffer> _products = new();

        public List<IapOffer> Products
        {
            get => _products;
            set => _products = value;
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        private void UpdateStoreIds()
        {
            var bundleId = UnityEditor.PlayerSettings.applicationIdentifier;
            foreach (var product in _products)
            {
                product.UpdateStoreIds(bundleId);
            }
        }
#endif
    }
}