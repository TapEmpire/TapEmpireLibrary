using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.Services
{
    public class NoAdsIapHandler : BaseIapHandler<DisableAdsProduct>
    {
        private readonly IAdsService _adsService;

        public NoAdsIapHandler(IAdsService adsService)
        {
            _adsService = adsService;
        }

        public override UniTask Handle(DisableAdsProduct product)
        {
            Debug.Log($"IAP NoAdsIapHandler Handle. iapSettings.DisableAd: {product.ProductId}");
            _adsService.DisableAds(true);
            return UniTask.CompletedTask;
        }
    }
}