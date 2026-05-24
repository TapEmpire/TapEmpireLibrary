#if TEL_METICA
using System.Threading;
using Cysharp.Threading.Tasks;
using Metica.ADS;
using Metica.SDK;
using R3;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public class MeticaInitializer
    {
        private readonly ReactiveProperty<bool> _isInitialized = new(false);

        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;
        public bool IsAdsEnabled { get; private set; }

        public async UniTask Initialize(CancellationToken cancellationToken = default)
        {
            if (_isInitialized.Value) return;

            MeticaSdk.CurrentUserId = GetUserId();
            IsAdsEnabled = await MeticaAds.InitializeAsync(new MeticaConfiguration());
            cancellationToken.ThrowIfCancellationRequested();
            _isInitialized.Value = true;
        }

        private static string GetUserId()
        {
#if UNITY_ANDROID
            return SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IOS
            return UnityEngine.iOS.Device.vendorIdentifier;
#else
            return "UnknownDevice";
#endif
        }
    }
}
#endif
