#if TEL_META
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Facebook.Unity;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class FacebookService : Initializable, IFacebookService
    {
        private IConsentService _consentService;
        private IAdsService _adsService;
        private IProgressService _progressService;
        private IIapService _iapService;

        private readonly CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IConsentService consentService, IAdsService adsService, IProgressService progressService,
            IIapService iapService)
        {
            _consentService = consentService;
            _adsService = adsService;
            _progressService = progressService;
            _iapService = iapService;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (!_adsService.Settings.AdsAnalyticsSettings.EnableMeta) return;

            await _consentService.IsResolved.WaitTrue(cancellationToken);

            var isPersonalized = _consentService.IsPersonalized.CurrentValue;
            await InitializeFacebook(isPersonalized, cancellationToken);

            new FacebookAdsModule(_adsService).AddTo(_disposables);
            new FacebookAdsSignalsModule(_adsService, _progressService, _iapService).AddTo(_disposables);
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
            base.OnRelease();
        }

        private static async UniTask InitializeFacebook(bool isPersonalized, CancellationToken cancellationToken)
        {
            if (!FB.IsInitialized)
            {
                var completion = new UniTaskCompletionSource();
                FB.Init(() => completion.TrySetResult(), OnHideUnity);
                await completion.Task.AttachExternalCancellation(cancellationToken);
            }
            FB.ActivateApp();

#if UNITY_IOS && !UNITY_EDITOR
            FB.Mobile.SetAdvertiserTrackingEnabled(isPersonalized);
#endif
        }

        private static void OnHideUnity(bool isGameShown)
        {
            Time.timeScale = isGameShown ? 1 : 0;
        }
    }
}
#endif
