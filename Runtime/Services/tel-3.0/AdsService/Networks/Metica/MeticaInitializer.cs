#if TEL_METICA
using Cysharp.Threading.Tasks;
using Metica.ADS;
using Metica.SDK;
using Metica.Unity;
using R3;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public class MeticaInitializer
    {
        private readonly ReactiveProperty<bool> _isInitialized = new(false);

        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;
        public bool IsMeticaEnabled { get; private set; }

        public async UniTask Initialize(MeticaUnitySdk prefab)
        {
            if (_isInitialized.Value) return;

            Object.Instantiate(prefab);

            MeticaSdk.CurrentUserId = GetUserId();
            IsMeticaEnabled = await MeticaAds.InitializeAsync(new MeticaConfiguration());
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
