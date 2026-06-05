#if TEL_METICA
using Cysharp.Threading.Tasks;
using Metica.ADS;
using Metica.SDK;
using Metica.Unity;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MeticaInitializer
    {
        private bool _initialized;

        public bool IsMeticaEnabled { get; private set; }

        public async UniTask Initialize(MeticaUnitySdk prefab)
        {
            if (_initialized) return;

            Object.Instantiate(prefab);

            MeticaSdk.CurrentUserId = GetUserId();
            IsMeticaEnabled = await MeticaAds.InitializeAsync(new MeticaConfiguration());
            _initialized = true;
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
