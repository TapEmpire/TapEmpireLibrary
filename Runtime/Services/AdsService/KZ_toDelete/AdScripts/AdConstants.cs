using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AdConstants
{
    public static bool IsDebugBuild => Debug.isDebugBuild; //  || PlayerPrefs.GetInt("DevMode", 0).Equals(1);

    public static string GetAdmobTestID(AdFormat format)
    {
#if UNITY_ANDROID
        switch (format)
        {
            case AdFormat.Banner: return "ca-app-pub-3940256099942544/6300978111";
            case AdFormat.MREC: return "ca-app-pub-3940256099942544/6300978111";
            case AdFormat.Interstitial: return "ca-app-pub-3940256099942544/1033173712";
            case AdFormat.Rewarded: return "ca-app-pub-3940256099942544/5224354917";
            case AdFormat.AppOpen: return "ca-app-pub-3940256099942544/9257395921";
            case AdFormat.NativeAd: return "ca-app-pub-3940256099942544/2247696110";
            default: return null;
        }
#elif UNITY_IOS
        switch (format)
        {
            case AdFormat.Banner: return "ca-app-pub-3940256099942544/2934735716";
            case AdFormat.MREC: return "ca-app-pub-3940256099942544/2934735716";
            case AdFormat.Interstitial: return "ca-app-pub-3940256099942544/4411468910";
            case AdFormat.Rewarded: return "ca-app-pub-3940256099942544/1712485313";
            case AdFormat.AppOpen: return "ca-app-pub-3940256099942544/5575463023";
            case AdFormat.NativeAd: return "ca-app-pub-3940256099942544/3986624511";
            default: return null;
        }
#else
        return "unexpected_platform";
#endif
    }
}
