#if UNITY_ANDROID
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Crashlytics;
using Google.Play.Review;

namespace TapEmpire.Services
{
    public class AndroidReviewManager : IReviewManager
    {
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;

        public AndroidReviewManager()
        {
            _reviewManager = new ReviewManager();
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
                // _progressService.SetRateMe(false);
                Crashlytics.LogException(new System.Exception(requestFlowOperation.Error.ToString()));
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
                    // _progressService.SetRateMe(false);
                    Crashlytics.LogException(new System.Exception(launchFlowOperation.Error.ToString()));
                }
                else
                {
                    // _progressService.SetRateMe(true);
                }
            }
            else
            {
                Crashlytics.LogException(new System.Exception($"RateMe PlayReviewInfo == null"));
            }
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
        }
    }
}
#endif
