using Zenject;

namespace TapEmpire.Services
{
    public interface IAdsInterstitialStrategy
    {
        void Configure(AdsSettings adsSettings, DiContainer diContainer);
        void UpdateInterstitialAds();
        bool IsShouldShowAds(int levelIndex);
    }
}