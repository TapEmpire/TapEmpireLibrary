using System.Collections.Generic;

namespace TapEmpire.Services
{
    public class AdsRuntimeScenario
    {
        public bool IsEnabled;
        public bool EnableAppOpen;
        public bool ShouldWaitAppOpen;
        public List<int> InterstitialAfterLevels;
        public bool ShowBanner;
        public int FromLevel;
    }
}