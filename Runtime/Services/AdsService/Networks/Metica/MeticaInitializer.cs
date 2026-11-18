#if TEL_METICA
using System.Threading;
using Cysharp.Threading.Tasks;
using Metica;
using Metica.Ads;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MeticaInitializer
    {
        public bool IsTrial { get; private set; }

        public async UniTask<bool> Initialize(AdsConfig config, bool isPersonalized, CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread(cancellationToken);

            MeticaAds.RevenueCallbackDelivery = CallbackDelivery.NativeThread;
            MeticaSdk.Ads.SetHasUserConsent(isPersonalized);

            var initConfig = new MeticaInitConfig(config.MeticaApiKey, config.MeticaAppId, SystemInfo.deviceUniqueIdentifier);
            var mediationInfo = new MeticaMediationInfo(MeticaMediationInfo.MeticaMediationType.MAX, config.AppLovin.AppKey);
            var response = await MeticaSdk.InitializeAsync(initConfig, mediationInfo).AsUniTask().AttachExternalCancellation(cancellationToken);

            IsTrial = response.SmartFloors?.UserGroup == MeticaUserGroup.TRIAL;
            return MeticaSdk.Ads.Max.IsSuccessfullyInitialized();
        }
    }
}
#endif
