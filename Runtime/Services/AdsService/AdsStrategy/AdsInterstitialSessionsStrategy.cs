using System;
using Zenject;

namespace TapEmpire.Services
{
    public class AdsInterstitialSessionsStrategy : IAdsInterstitialStrategy
    {
        private AdsSettings _adsSettings;
        private ISessionService _sessionService;
        private IProgressService _progressService;

        private float _levelIntervalShowing;
        private DateTime _lastAdShowTime;

        public void Configure(AdsSettings adsSettings, DiContainer diContainer)
        {
            _adsSettings = adsSettings;
            _sessionService = diContainer.Resolve<ISessionService>();
            _progressService = diContainer.Resolve<IProgressService>();

            ResetSessionProgressIfNeeded();
            _sessionService.ResetTotalInactiveTime();

            _lastAdShowTime = DateTime.UtcNow;
            _levelIntervalShowing = CalculateNextInterval();
        }

        public void UpdateInterstitialAds()
        {
            _lastAdShowTime = DateTime.UtcNow;
            _levelIntervalShowing = CalculateNextInterval();
        }

        public bool IsShouldShowAds(int levelIndex)
        {
            if (ShouldShowAdBasedOnLevels() || ShouldShowAdBasedOnTime())
            {
                _progressService.SetCompletedLevelsForOneSession(0);
                return true;
            }

            return false;
        }

        private void ResetSessionProgressIfNeeded()
        {
            var inactiveMinutes = _sessionService.GetTotalInactiveTime().TotalMinutes;
            if (inactiveMinutes >= _adsSettings.AdsInterstitialSessionData.SessionDuration)
            {
                _progressService.SetCompletedLevelsForOneSession(0);
                _progressService.SetShowingAdCount(0);
                _progressService.SetListIndexShowingAd(0);
            }
        }

        private float CalculateNextInterval()
        {
            var adsShowingCount = _progressService.GetShowingAdCount();
            var listIndex = _progressService.GetListIndexShowingAd();

            if (listIndex >= _adsSettings.AdsInterstitialSessionData.AdsInterstitialShowingData.Count)
            {
                return _adsSettings.AdsInterstitialSessionData.AdsInterstitialShowingData[^1].LevelIntervalShowing;
            }

            var data = _adsSettings.AdsInterstitialSessionData.AdsInterstitialShowingData[listIndex];
            adsShowingCount++;

            if (adsShowingCount >= data.AdsShowing)
            {
                listIndex++;
                _progressService.SetListIndexShowingAd(listIndex);
                adsShowingCount = 0;
            }

            _progressService.SetShowingAdCount(adsShowingCount);
            return data.LevelIntervalShowing;
        }

        private bool ShouldShowAdBasedOnLevels()
        {
            return _progressService.GetCompletedLevelsForOneSession() >= _levelIntervalShowing;
        }

        private bool ShouldShowAdBasedOnTime()
        {
            var elapsedSeconds = (DateTime.UtcNow - _lastAdShowTime).TotalSeconds;
            var listIndex = _progressService.GetListIndexShowingAd();

            if (listIndex >= _adsSettings.AdsInterstitialSessionData.AdsInterstitialShowingData.Count)
            {
                listIndex = _adsSettings.AdsInterstitialSessionData.AdsInterstitialShowingData.Count - 1;
                _progressService.SetListIndexShowingAd(listIndex);
            }

            var adsTimer = _adsSettings.AdsInterstitialSessionData.AdsInterstitialShowingData[listIndex].AdsTimerShowing;

            if (elapsedSeconds >= adsTimer)
            {
                var adsShowingCount = _progressService.GetShowingAdCount();
                adsShowingCount++;
                _progressService.SetShowingAdCount(adsShowingCount);
                return true;
            }

            return false;
        }
    }
}
