using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;

namespace TapEmpire.Experimental
{
    public static class AdmobInitializer
    {
        private static bool _initialized;

        public static async UniTask Initialize(
            bool isPersonalized,
            bool isForFamily,
            bool testMode = false,
            CancellationToken cancellationToken = default)
        {
            if (_initialized) return;

            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            var configuration = new RequestConfiguration
            {
                PublisherPrivacyPersonalizationState = isPersonalized
                    ? PublisherPrivacyPersonalizationState.Enabled
                    : PublisherPrivacyPersonalizationState.Disabled,
                TagForUnderAgeOfConsent = isForFamily
                    ? TagForUnderAgeOfConsent.True
                    : TagForUnderAgeOfConsent.False,
            };

            if (testMode)
            {
                configuration.TestDeviceIds = await GetTestDeviceIds(cancellationToken);
            }

            MobileAds.SetRequestConfiguration(configuration);

            var completion = new UniTaskCompletionSource();
            MobileAds.Initialize(_ => completion.TrySetResult());

            await completion.Task.AttachExternalCancellation(cancellationToken);
            _initialized = true;
        }

        private static async UniTask<List<string>> GetTestDeviceIds(CancellationToken cancellationToken)
        {
            var gaid = await AdvertisingId.Get(cancellationToken);
            return string.IsNullOrEmpty(gaid)
                ? new List<string>()
                : new List<string> { gaid };
        }
    }
}
