using System;
using System.Collections.Generic;

namespace TapEmpire.Services
{
    [Serializable]
    public class SessionData
    {
        public float Duration = 1800.0f;
        public List<InterstitialData> InterstitialData;
    }

    [Serializable]
    public class InterstitialData
    {
        public int Ads;
        public int Interval;
        public float Timer;
    }
}