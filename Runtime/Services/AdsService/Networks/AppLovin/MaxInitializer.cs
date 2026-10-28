using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public static class MaxInitializer
    {
        private static bool _initialized;

        public static async UniTask<bool> Initialize(
            bool isPersonalized,
            bool testMode = false,
            CancellationToken cancellationToken = default)
        {
            if (_initialized) return true;

            if (testMode)
            {
                await SetTestDeviceIds(cancellationToken);
            }

            MaxSdkBase.InvokeEventsOnUnityMainThread = false;
            MaxSdk.SetVerboseLogging(testMode);
            MaxSdk.SetHasUserConsent(isPersonalized);

            var completion = new UniTaskCompletionSource<bool>();

            void OnInitialized(MaxSdkBase.SdkConfiguration configuration)
            {
                MaxSdkCallbacks.OnSdkInitializedEvent -= OnInitialized;
                UniTaskUtility.RunOnMainThread(() => completion.TrySetResult(configuration.IsSuccessfullyInitialized));
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += OnInitialized;
            MaxSdk.InitializeSdk();

            var isSuccessful = await completion.Task.AttachExternalCancellation(cancellationToken);
            _initialized = true;

            return isSuccessful;
        }

        private static async UniTask SetTestDeviceIds(CancellationToken cancellationToken)
        {
            var gaid = await AdvertisingId.Get(cancellationToken);
            if (!string.IsNullOrEmpty(gaid))
            {
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(new[] { gaid });
            }
        }
    }
}
