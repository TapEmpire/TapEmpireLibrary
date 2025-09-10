using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class NoAdsIapHandler : BaseIapHandler<DisableAdsProduct>
    {
        private IAdsService _adsService;

        public override bool IsConsumable => false;

        public override void Initialize(DiContainer diContainer)
        {
            _adsService = diContainer.Resolve<IAdsService>();
        }

        public override UniTask Handle(DisableAdsProduct product)
        {
            Debug.Log($"IAP NoAdsIapHandler Handle. iapSettings.DisableAd: {product.ProductId}");
            _adsService.DisableAds(true);
            return UniTask.CompletedTask;
        }
    }
}