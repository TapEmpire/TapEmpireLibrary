using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Crashlytics;
using Google.Play.Review;
using TapEmpire.Services;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class RateMeService : Initializable, IRateMeService
    {
        [NonSerialized] private ReviewManager _reviewManager;
        [NonSerialized] private PlayReviewInfo _playReviewInfo;

        public bool HasRated => _progressService.GetRateMe();

        private IProgressService _progressService = null;

        [Inject]
        private void Construct(IProgressService progressService)
        {
            _progressService = progressService;
        }
        
        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _reviewManager = new ReviewManager();
            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _reviewManager = null;
        }

        public async UniTask RateMeAsync(CancellationToken cancellationToken)
        {
            await RateCoroutine().ToUniTask(cancellationToken: cancellationToken);
        }

        private IEnumerator RateCoroutine()
        {
            yield return RequestFlowCoroutine();
            
            yield return PlayReviewFlowCoroutine();
        }

        private IEnumerator RequestFlowCoroutine()
        {
            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                _progressService.SetRateMe(false);
                Crashlytics.LogException(new Exception(requestFlowOperation.Error.ToString()));
                yield break;
            }
            
            _playReviewInfo = requestFlowOperation.GetResult();
        }

        private IEnumerator PlayReviewFlowCoroutine()
        {
            if (_playReviewInfo != null)
            {
                var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
                yield return launchFlowOperation;
                _playReviewInfo = null; // Reset the object
                if (launchFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    _progressService.SetRateMe(false);
                    Crashlytics.LogException(new Exception(launchFlowOperation.Error.ToString()));
                }
                else
                {
                    _progressService.SetRateMe(true);
                }
            }
            else
            {
                Crashlytics.LogException(new Exception($"RateMe PlayReviewInfo == null"));
            }
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
        }
    }
}