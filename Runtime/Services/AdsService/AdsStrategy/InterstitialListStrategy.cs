using System.Linq;
using Zenject;

namespace TapEmpire.Services
{
    public class InterstitialListStrategy : IInterstitialStrategy
    {
        private AdsSettings _adsSettings;
        
        public void Configure(AdsSettings adsSettings, DiContainer diContainer)
        {
            _adsSettings = adsSettings;
        }

        public void UpdateInterstitialAds()
        {
            
        }

        public bool IsShouldShowAds(int levelIndex)
        {
            return _adsSettings.InterstitialAfterLevels.Any(interstitialLevel => interstitialLevel == levelIndex + 1);
        }
    }
}