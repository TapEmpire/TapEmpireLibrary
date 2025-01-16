using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace TapEmpire.Services.AdsStrategy
{
    [Serializable]
    public class AdsInterstitialSessionData
    {
        public float SessionDuration = 30.0f; //minute
        public List<AdsInterstitialShowingData> AdsInterstitialShowingData;
    }

    [Serializable]
    public class AdsInterstitialShowingData
    {
        public int AdsShowing;
        public float LevelIntervalShowing;
        public float AdsTimerShowing;
    }
}