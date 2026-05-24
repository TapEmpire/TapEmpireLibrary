using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Experimental
{
    public static class MaxInitializer
    {
        private static bool _initialized;

        public static async UniTask Initialize(
            bool isPersonalized,
            bool testMode = false,
            CancellationToken cancellationToken = default)
        {
            if (_initialized) return;

            if (testMode)
            {
                await SetTestDeviceIds(cancellationToken);
            }

            MaxSdk.SetVerboseLogging(testMode);
            MaxSdk.SetHasUserConsent(isPersonalized);

            var completion = new UniTaskCompletionSource();

            void OnInitialized(MaxSdkBase.SdkConfiguration _)
            {
                MaxSdkCallbacks.OnSdkInitializedEvent -= OnInitialized;
                completion.TrySetResult();
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += OnInitialized;
            MaxSdk.InitializeSdk();

            await completion.Task.AttachExternalCancellation(cancellationToken);
            _initialized = true;
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
