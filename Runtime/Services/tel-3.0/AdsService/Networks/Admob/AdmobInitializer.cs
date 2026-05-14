using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using R3;

namespace TapEmpire.Experimental
{
    public class AdmobInitializer
    {
        private readonly ReactiveProperty<bool> _isInitialized = new(false);

        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;

        public async UniTask Initialize(
            bool isPersonalized,
            bool testMode = false,
            CancellationToken cancellationToken = default)
        {
            if (_isInitialized.Value)
            {
                return;
            }

            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            var configuration = new RequestConfiguration
            {
                PublisherPrivacyPersonalizationState = isPersonalized
                    ? PublisherPrivacyPersonalizationState.Enabled
                    : PublisherPrivacyPersonalizationState.Disabled,
            };

            if (testMode)
            {
                configuration.TestDeviceIds = await GetTestDeviceIds(cancellationToken);
            }

            MobileAds.SetRequestConfiguration(configuration);

            var completion = new UniTaskCompletionSource();
            MobileAds.Initialize(_ => completion.TrySetResult());

            await completion.Task.AttachExternalCancellation(cancellationToken);
            _isInitialized.Value = true;
        }

        private static async UniTask<List<string>> GetTestDeviceIds(CancellationToken cancellationToken)
        {
            var gaid = await AdvertisingId.Get(cancellationToken);
            return string.IsNullOrEmpty(gaid)
                ? new List<string>()
                : new List<string> { gaid };
        }

        // Unused scaffold: legacy AdsManager.IsForFamily drove TagForUnderAgeOfConsent; call after `new RequestConfiguration { ... }` when wiring back.
        private static void ApplyIsForFamily(RequestConfiguration configuration, bool isForFamily)
        {
            configuration.TagForUnderAgeOfConsent = isForFamily
                ? TagForUnderAgeOfConsent.True
                : TagForUnderAgeOfConsent.False;
        }
    }
}
