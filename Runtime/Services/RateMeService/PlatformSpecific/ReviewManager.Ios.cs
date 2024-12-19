#if UNITY_IOS
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.iOS;

namespace TapEmpire.Services
{
    public class IosReviewManager : IReviewManager
    {
        public UniTask RateMeAsync(CancellationToken cancellationToken)
        {
            Device.RequestStoreReview();
            return UniTask.CompletedTask;
        }
    }
}
#endif