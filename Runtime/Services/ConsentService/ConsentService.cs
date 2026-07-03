using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using R3;
using TapEmpire.Services;
using UnityEngine;
#if UNITY_IOS && TEL_ATT && !UNITY_EDITOR
using Unity.Advertisement.IosSupport;
#endif

namespace TapEmpire.Services
{
    [System.Serializable]
    public class ConsentService : Initializable, IConsentService
    {
        // IAB TCF v2 personalization purposes (1, 2, 3, 4, 7, 9, 10) — string is 0-indexed.
        private static readonly int[] PurposeIndices = { 0, 1, 2, 3, 6, 8, 9 };

        // UMP talks to Google servers; on a blocked/degraded network a callback may never
        // fire and would hang startup. Bound the wait and continue with safe defaults.
        private const float ConsentTimeoutSeconds = 12f;

        [SerializeField] private ConsentSettings _settings;

        private readonly ReactiveProperty<bool> _isResolved = new(false);
        private readonly ReactiveProperty<bool> _isPersonalized = new(false);
        private readonly ReactiveProperty<bool> _isEurArea = new(false);
        private readonly ReactiveProperty<bool> _isAttGranted = new(false);

        public ReadOnlyReactiveProperty<bool> IsResolved => _isResolved;
        public ReadOnlyReactiveProperty<bool> IsPersonalized => _isPersonalized;
        public ReadOnlyReactiveProperty<bool> IsEurArea => _isEurArea;
        public ReadOnlyReactiveProperty<bool> IsAttGranted => _isAttGranted;
        public bool IsForFamily => _settings.IsForFamily;

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            await GatherAsync(cancellationToken);
        }

        private async UniTask GatherAsync(CancellationToken cancellationToken)
        {
#if UNITY_IOS && TEL_ATT && !UNITY_EDITOR
            if (_settings.EnableAtt)
            {
                _isAttGranted.Value = await GatherAtt(cancellationToken);
            }
#endif
            if (_settings.EnableUmp)
            {
                await GatherConsent(cancellationToken);
            }

            _isResolved.Value = true;
        }

        private async UniTask GatherConsent(CancellationToken cancellationToken)
        {
            Debug.Log("[Consent] Fetching UMP consent");
            var completion = new UniTaskCompletionSource();

            ConsentInformation.Update(CreateRequestParameters(), updateError =>
            {
                if (updateError != null)
                {
                    LogAndComplete(completion, $"UMP update failed: {updateError.Message}");
                    return;
                }

                if (ConsentInformation.ConsentStatus.Equals(ConsentStatus.Required))
                {
                    ConsentForm.Load((form, loadError) =>
                    {
                        if (form == null)
                        {
                            LogAndComplete(completion, $"Failed to load consent form: {loadError?.Message}");
                            return;
                        }

                        form.Show(showError =>
                            LogAndComplete(completion, showError == null ? "Consent form shown" : $"Failed to show consent form: {showError.Message}"));
                    });
                }
                else
                {
                    LogAndComplete(completion, "Consent already resolved or not required");
                }
            });

            try
            {
                // Dispose the timer with the CTS so a leaked PlayerLoopTimer can't Cancel() the
                // disposed source later and throw ObjectDisposedException in the player loop.
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                using var timeoutTimer = cts.CancelAfterSlim(TimeSpan.FromSeconds(ConsentTimeoutSeconds));
                await completion.Task.AttachExternalCancellation(cts.Token);
                ReadConsentState();
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                // Timed out waiting for a UMP callback — fail open with privacy-safe, non-personalized defaults.
                Debug.LogWarning($"[Consent] UMP consent timed out ({ConsentTimeoutSeconds:F0}s) — continuing non-personalized.");
                _isPersonalized.Value = false;
                _isEurArea.Value = false;
            }
        }

        private void ReadConsentState()
        {
            if (ConsentInformation.ConsentStatus.Equals(ConsentStatus.NotRequired) ||
                ConsentInformation.ConsentStatus.Equals(ConsentStatus.Unknown))
            {
                _isPersonalized.Value = true;
                _isEurArea.Value = false;
                Debug.Log($"[Consent] isPersonalized={_isPersonalized.Value}, isEurArea={_isEurArea.Value}");
                return;
            }

            string purposeConsents = ApplicationPreferences.GetString("IABTCF_PurposeConsents");
            int gdprApplies = ApplicationPreferences.GetInt("IABTCF_gdprApplies");

            _isEurArea.Value = gdprApplies == 1;
            _isPersonalized.Value = ArePurposesGranted(purposeConsents);

            Debug.Log($"[Consent] isPersonalized={_isPersonalized.Value}, isEurArea={_isEurArea.Value}, gdpr={gdprApplies}, purposes={purposeConsents}");
        }

        private static bool ArePurposesGranted(string purposeConsents)
        {
            if (string.IsNullOrEmpty(purposeConsents) || purposeConsents.Length < 10)
            {
                return false;
            }
            return PurposeIndices.All(i => purposeConsents[i] == '1');
        }

        private ConsentRequestParameters CreateRequestParameters()
        {
            var parameters = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = _settings.IsForFamily,
            };

            if (_settings.TestDeviceHashedIds.Count > 0)
            {
                parameters.ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.Disabled,
                    TestDeviceHashedIds = _settings.TestDeviceHashedIds,
                };
            }
            return parameters;
        }

        private static void LogAndComplete(UniTaskCompletionSource completion, string info)
        {
            Debug.Log($"[Consent] {info}");
            completion.TrySetResult();
        }

#if UNITY_IOS && TEL_ATT && !UNITY_EDITOR
        private async UniTask<bool> GatherAtt(CancellationToken cancellationToken)
        {
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                await UniTask.WaitWhile(
                    () => ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED,
                    cancellationToken: cancellationToken);
                status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            }
            Debug.Log($"[Consent] ATT status = {status}");
            return status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
        }
#endif
    }
}
