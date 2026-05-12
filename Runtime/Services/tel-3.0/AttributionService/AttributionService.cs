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

        public ReadOnlyReactiveProperty<string> CampaignName => _campaignName;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            Adjust.InitSdk(BuildConfig());
            Adjust.GetAttribution(OnAttribution);
#if UNITY_ANDROID
            Application.deepLinkActivated += OnDeeplink;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeeplink(Application.absoluteURL);
            }
#endif
            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
#if UNITY_ANDROID
            Application.deepLinkActivated -= OnDeeplink;
#endif
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
                _settings.LogLevel == AdjustLogLevel.Suppress)
            {
                LogLevel = _settings.LogLevel,
                IsSendingInBackgroundEnabled = _settings.SendInBackground,
                IsDeferredDeeplinkOpeningEnabled = _settings.LaunchDeferredDeeplink,
                DefaultTracker = _settings.DefaultTracker,
                IsCoppaComplianceEnabled = _settings.CoppaCompliance,
                IsCostDataInAttributionEnabled = _settings.CostDataInAttribution,
                IsPreinstallTrackingEnabled = _settings.PreinstallTracking,
                PreinstallFilePath = _settings.PreinstallFilePath,
                IsAdServicesEnabled = _settings.AdServices,
                IsIdfaReadingEnabled = _settings.IdfaReading,
                IsLinkMeEnabled = _settings.LinkMe,
                IsSkanAttributionEnabled = _settings.SkanAttribution,
            };
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

#if UNITY_ANDROID
        private static void OnDeeplink(string url)
        {
            Adjust.ProcessDeeplink(new AdjustDeeplink(url));
        }
#endif
    }
}
