using System;
using System.Collections.Generic;
using System.Threading;
using AdjustSdk;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class AttributionService : Initializable, IAttributionService
    {
        [SerializeField] private AttributionSettings _settings;

        private readonly ReactiveProperty<string> _campaignName = new(string.Empty);
        private readonly CompositeDisposable _disposables = new();

        private IConsentService _consentService;

        public ReadOnlyReactiveProperty<string> CampaignName => _campaignName;

        [Inject]
        private void Construct(IConsentService consentService)
        {
            _consentService = consentService;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            SubscribeDeepLinks();

            await _consentService.IsResolved.WaitTrue(cancellationToken);

            PassDmaConsent();
            Adjust.InitSdk(BuildConfig());
            Adjust.GetAttribution(OnAttribution);
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
            base.OnRelease();
        }

        public static void TrackEvent(string eventToken, IReadOnlyDictionary<string, string> callbackParameters = null)
        {
            var evt = new AdjustEvent(eventToken);
            AppendCallbackParameters(evt, callbackParameters);
            Adjust.TrackEvent(evt);
        }

        public static void TrackRevenue(
            string eventToken,
            double revenue,
            string isoCurrency,
            string productId,
            IReadOnlyDictionary<string, string> callbackParameters = null)
        {
            var evt = new AdjustEvent(eventToken);
            evt.SetRevenue(revenue, isoCurrency);
            evt.ProductId = productId;
            AppendCallbackParameters(evt, callbackParameters);
            Adjust.TrackEvent(evt);
        }

        public static void TrackAdRevenue(
            string source,
            double revenue,
            string currency,
            string network = null,
            string adRevenueUnit = null,
            IReadOnlyDictionary<string, string> partnerParameters = null)
        {
            var adRevenue = new AdjustAdRevenue(source);
            adRevenue.SetRevenue(revenue, currency);
            adRevenue.AdRevenueNetwork = network;
            adRevenue.AdRevenueUnit = adRevenueUnit;
            AppendPartnerParameters(adRevenue, partnerParameters);
            Adjust.TrackAdRevenue(adRevenue);
        }

        public static void VerifyAndroidPurchase(string productId, string purchaseToken, Action<bool> callback)
        {
            var purchase = new AdjustPlayStorePurchase(productId, purchaseToken);
            Adjust.VerifyPlayStorePurchase(purchase, result =>
                callback?.Invoke(result.VerificationStatus == "success"));
        }

        public static void VerifyApplePurchase(string transactionId, string productId, Action<bool> callback)
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

        private static void AppendPartnerParameters(AdjustAdRevenue adRevenue, IReadOnlyDictionary<string, string> parameters)
        {
            if (parameters == null) return;

            foreach (var pair in parameters)
            {
                adRevenue.AddPartnerParameter(pair.Key, pair.Value);
            }
        }

        private static void AppendCallbackParameters(AdjustEvent evt, IReadOnlyDictionary<string, string> parameters)
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

        private void PassDmaConsent()
        {
            var eea = _consentService.IsEurArea.CurrentValue ? "1" : "0";
            var personalized = _consentService.IsPersonalized.CurrentValue ? "1" : "0";
            var sharing = new AdjustThirdPartySharing(null);
            sharing.AddGranularOption("google_dma", "eea", eea);
            sharing.AddGranularOption("google_dma", "ad_personalization", personalized);
            sharing.AddGranularOption("google_dma", "ad_user_data", personalized);
            Adjust.TrackThirdPartySharing(sharing);
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
