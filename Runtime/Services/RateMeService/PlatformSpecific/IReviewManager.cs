using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IReviewManager
    {
        public UniTask RateMeAsync(CancellationToken cancellationToken);
    }
}