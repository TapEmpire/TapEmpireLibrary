namespace TapEmpire.Services
{
    public class AdsInterstitialStrategyContext
    {
        private IAdsInterstitialStrategy _adsInterstitialStrategy;
        
        public void SetAdsStrategy(IAdsInterstitialStrategy adsInterstitialStrategy)
        {
            _adsInterstitialStrategy = adsInterstitialStrategy;
        }

        public void UpdateInterstitialAds()
        {
            _adsInterstitialStrategy.UpdateInterstitialAds();
        }
        
        public bool IsShouldShowAds(int levelIndex)
        {
            return _adsInterstitialStrategy.IsShouldShowAds(levelIndex);
        }
    }
}