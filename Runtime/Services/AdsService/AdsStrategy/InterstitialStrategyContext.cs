namespace TapEmpire.Services
{
    public class InterstitialStrategyContext
    {
        private IInterstitialStrategy _interstitialStrategy;
        
        public void SetAdsStrategy(IInterstitialStrategy interstitialStrategy)
        {
            _interstitialStrategy = interstitialStrategy;
        }

        public void UpdateInterstitialAds()
        {
            _interstitialStrategy.UpdateInterstitialAds();
        }
        
        public bool IsShouldShowAds(int levelIndex)
        {
            return _interstitialStrategy.ShouldShowAds(levelIndex);
        }
    }
}