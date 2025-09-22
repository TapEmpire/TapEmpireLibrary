using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class ConsentManager
{
    public static bool IsFetchingIos = false;
    public static bool IsFetching = false;
    public static bool ConsentStillRequired => ConsentInformation.ConsentStatus.Equals(ConsentStatus.Unknown);
    public static bool isPersonalized = true;
    public static bool isEurArea = false;

    public static void GatherConsent(bool debugConsent, bool targetUnderAge, Action<ConsentStatus, string> onComplete)
    {
        if (IsFetching)
            return;
        else
        {
            IsFetching = true;
            onComplete += delegate { IsFetching = false; };
        }

        MonetizationLogger.Log("Fetching Consent");

        ConsentRequestParameters requestParameters = new ConsentRequestParameters();
        requestParameters.TagForUnderAgeOfConsent = targetUnderAge;
        if (debugConsent)
            requestParameters.ConsentDebugSettings = GetDebugSettings();

        ConsentInformation.Update(requestParameters, (FormError updateError) =>
        {
            if (updateError != null)
            {
                onComplete(ConsentInformation.ConsentStatus, updateError.Message);
                return;
            }

            if (ConsentInformation.ConsentStatus.Equals(ConsentStatus.Required))
            {
                ConsentForm.Load((ConsentForm form, FormError error) =>
                {
                    if (form != null)
                        ShowConsentForm(form, onComplete);
                    else
                        onComplete(ConsentInformation.ConsentStatus, "Failed to load consent form with error: " + error == null ? "unknown error" : error.Message);
                });
            }
            else
            {
                UpdateConsentMode();
                onComplete(ConsentInformation.ConsentStatus, "Consent has already been gathered or not required.");
            }


        });
    }

    static void ShowConsentForm(ConsentForm _consentForm, Action<ConsentStatus, string> onComplete)
    {
        Time.timeScale = 0;
        _consentForm.Show(
             (FormError error) =>
             {
                 Time.timeScale = 1;
                 if (error == null)
                 {
                     UpdateConsentMode();
                     onComplete(ConsentInformation.ConsentStatus, "Consent showed successfully");
                 }

                 else
                     onComplete(ConsentInformation.ConsentStatus, "Failed to show consent form with error: " + error.Message);
             });
    }

    static ConsentDebugSettings GetDebugSettings()
    {
        List<string> TestDeviceIds = new List<string>()
        {
            AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
            "702815ACFC14FF222DA1DC767672A573"
#endif
        };

        return new ConsentDebugSettings
        {
            DebugGeography = DebugGeography.Disabled,
            TestDeviceHashedIds = TestDeviceIds,
        };
    }


    static void UpdateConsentMode()
    {
        if (ConsentInformation.ConsentStatus.Equals(ConsentStatus.NotRequired) || ConsentInformation.ConsentStatus.Equals(ConsentStatus.Unknown))
        {
            isPersonalized = true;
            isEurArea = false;
            MonetizationLogger.Log($"isPersonalized = {isPersonalized}, isEurArea = {isEurArea}");
            return;
        }

        MonetizationLogger.Log("Fetching PurposeConsents");
        string purposeConsents = ApplicationPreferences.GetString("IABTCF_PurposeConsents");
        int gdprApplies = ApplicationPreferences.GetInt("IABTCF_gdprApplies");

        MonetizationLogger.Log($"PurposeConsents = {purposeConsents}, gdprApplies = {gdprApplies}");

        isEurArea = gdprApplies.Equals(1);

        if (!string.IsNullOrEmpty(purposeConsents) && purposeConsents.Length >= 10)
        {
            char[] aquiredPurpose = new char[7] { purposeConsents[0], purposeConsents[1], purposeConsents[2], purposeConsents[3], purposeConsents[6], purposeConsents[8], purposeConsents[9] };
            string targetPurpose = "1111111"; // Requirement of Personalization
            isPersonalized = CompareStringWithChars(targetPurpose, aquiredPurpose);
        }
        else
            isPersonalized = false;

        MonetizationLogger.Log($"isPersonalized = {isPersonalized}, isEurArea = {isEurArea}");
    }

    static bool CompareStringWithChars(string consent, char[] aquired)
    {
        if (!consent.Length.Equals(aquired.Length)) return false;

        for (int i = 0; i < consent.Length; i++)
        {
            if (!consent[i].Equals(aquired[i]))
                return false;
        }
        return true;
    }

    public async static UniTask GatherConsentIos()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (IsFetchingIos) return;

        IsFetchingIos = true;
        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

        if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking(); // 🔸 no callback available

            await UniTask.WaitWhile(() =>
                ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED);
            status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        }

        HandleATTStatus(status);
#else
        await UniTask.CompletedTask;
#endif
    }

#if UNITY_IOS
    public static void HandleATTStatus(ATTrackingStatusBinding.AuthorizationTrackingStatus status)
    {
        Debug.LogError($"ATT Status = {status}");

        switch (status)
        {
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                isPersonalized = false;
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED: // treat as yes temporarily
            default:
                isPersonalized = true;
                break;
        }

        IsFetchingIos = false;
    }
#endif
}
