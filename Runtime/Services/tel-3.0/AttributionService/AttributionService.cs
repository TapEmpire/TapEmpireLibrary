using System;
using System.Collections.Generic;
using System.Threading;
using AdjustSdk;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Experimental
{
    [System.Serializable]
    public class AttributionService : Initializable, IAttributionService
    {
        [SerializeField] private AttributionSettings _settings;

        private readonly ReactiveProperty<string> _campaignName = new(string.Empty);
        private readonly CompositeDisposable _disposables = new();

        public ReadOnlyReactiveProperty<string> CampaignName => _campaignName;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            SubscribeDeepLinks();
            Adjust.InitSdk(BuildConfig());
            Adjust.GetAttribution(OnAttribution);
            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
            base.OnRelease();
        }

        public void TrackEvent(string eventToken, IReadOnlyDictionary<string, string> callbackParameters = null)
        {
            var evt = new AdjustEvent(eventToken);
            AppendParameters(evt, callbackParameters);
            Adjust.TrackEvent(evt);
        }

        public void TrackRevenue(
            string eventToken,
            double revenue,
            string isoCurrency,
            string productId,
            IReadOnlyDictionary<string, string> callbackParameters = null)
        {
            var evt = new AdjustEvent(eventToken);
            evt.SetRevenue(revenue, isoCurrency);
            evt.ProductId = productId;
            AppendParameters(evt, callbackParameters);
            Adjust.TrackEvent(evt);
        }

        public void VerifyAndroidPurchase(string productId, string purchaseToken, Action<bool> callback)
        {
            var purchase = new AdjustPlayStorePurchase(productId, purchaseToken);
            Adjust.VerifyPlayStorePurchase(purchase, result =>
                callback?.Invoke(result.VerificationStatus == "success"));
        }

        public void VerifyApplePurchase(string transactionId, string productId, Action<bool> callback)
        {
            var purchase = new AdjustAppStorePurchase(transactionId, productId);
            Adjust.VerifyAppStorePurchase(purchase, result =>
                callback?.Invoke(result.VerificationStatus == "success"));
        }

        private AdjustConfig BuildConfig()
        {
            var environment = PlatformInfo.IsTestFlightOrSandboxReceipt()
                ? AdjustEnvironment.Sandbox
                : _settings.Environment;

            var config = new AdjustConfig(
                _settings.AppToken,
                environment,
                _settings.LogLevel == AdjustLogLevel.Suppress);

            config.LogLevel = _settings.LogLevel;
            config.IsSendingInBackgroundEnabled = _settings.SendInBackground;
            config.IsDeferredDeeplinkOpeningEnabled = _settings.LaunchDeferredDeeplink;
            config.DefaultTracker = _settings.DefaultTracker;
            config.IsCoppaComplianceEnabled = _settings.CoppaCompliance;
            config.IsCostDataInAttributionEnabled = _settings.CostDataInAttribution;
            config.IsPreinstallTrackingEnabled = _settings.PreinstallTracking;
            config.PreinstallFilePath = _settings.PreinstallFilePath;
            config.IsAdServicesEnabled = _settings.AdServices;
            config.IsIdfaReadingEnabled = _settings.IdfaReading;
            config.IsLinkMeEnabled = _settings.LinkMe;
            config.IsSkanAttributionEnabled = _settings.SkanAttribution;
#if UNITY_IOS && !UNITY_EDITOR
            config.AttConsentWaitingInterval = 120;
#endif
            return config;
        }

        private static void AppendParameters(AdjustEvent evt, IReadOnlyDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (var pair in parameters)
            {
                evt.AddCallbackParameter(pair.Key, pair.Value);
            }
        }

        private void OnAttribution(AdjustAttribution attribution)
        {
            _campaignName.Value = attribution?.Campaign ?? string.Empty;
        }

        private void SubscribeDeepLinks()
        {
#if UNITY_ANDROID
            Action<string> onDeepLink = url => Adjust.ProcessDeeplink(new AdjustDeeplink(url));
            
            Application.deepLinkActivated += onDeepLink;
            Disposable.Create(() => Application.deepLinkActivated -= onDeepLink).AddTo(_disposables);

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                onDeepLink(Application.absoluteURL);
            }
#endif
        }
    }
}
