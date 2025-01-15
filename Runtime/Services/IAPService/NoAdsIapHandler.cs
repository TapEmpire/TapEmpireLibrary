using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.Services
{
    public class NoAdsIapHandler : IIapHandler<PackIapSettings>
    {
        private readonly IAdsService _adsService;

        public NoAdsIapHandler(IAdsService adsService)
        {
            _adsService = adsService;
        }

        public async UniTask Handle(PackIapSettings iapSettings)
        {
            Debug.Log($"IAP NoAdsIapHandler Handle. iapSettings.DisableAd: {iapSettings.DisableAd}");

            if (iapSettings.DisableAd)
            {
                _adsService.DisableAds(true);
            }
        }
    }
}