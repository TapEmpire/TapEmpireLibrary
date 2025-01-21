using Zenject;

namespace TapEmpire.Services
{
    public interface IInterstitialStrategy
    {
        void Configure(AdsSettings adsSettings, DiContainer diContainer);
        void UpdateInterstitialAds();
        bool ShouldShowAds(int levelIndex);
    }
}