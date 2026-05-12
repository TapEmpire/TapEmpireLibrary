using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Experimental
{
    public class MaxInitializer
    {
        private readonly ReactiveProperty<bool> _isInitialized = new(false);

        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;

        public async UniTask Initialize(
            bool isPersonalized,
            bool verboseLogging = false,
            string testDeviceId = null,
            CancellationToken cancellationToken = default)
        {
            if (_isInitialized.Value)
            {
                return;
            }

            if (verboseLogging)
            {
                MaxSdk.SetVerboseLogging(true);
            }

            MaxSdk.SetHasUserConsent(isPersonalized);

            if (!string.IsNullOrEmpty(testDeviceId))
            {
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(new[] { testDeviceId });
            }

            var completion = new UniTaskCompletionSource();

            void OnInitialized(MaxSdkBase.SdkConfiguration _)
            {
                MaxSdkCallbacks.OnSdkInitializedEvent -= OnInitialized;
                completion.TrySetResult();
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += OnInitialized;
            MaxSdk.InitializeSdk();

            await completion.Task.AttachExternalCancellation(cancellationToken);
            _isInitialized.Value = true;
        }
    }
}
