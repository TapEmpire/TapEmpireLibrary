using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;

namespace TapEmpire.Services
{
    public static class AdmobInitializer
    {
        private static bool _initialized;

        // Test ads come from Google's sample ad unit IDs (see AdmobTestAdUnits), so no test-device registration is needed.
        public static async UniTask Initialize(
            bool isPersonalized,
            bool isForFamily,
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

            MobileAds.SetRequestConfiguration(configuration);

            var completion = new UniTaskCompletionSource();
            MobileAds.Initialize(_ => completion.TrySetResult());

            await completion.Task.AttachExternalCancellation(cancellationToken);
            _initialized = true;
        }
    }
}
