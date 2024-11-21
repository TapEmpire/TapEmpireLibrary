using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IRateMeService : IService
    {
        bool HasRated { get; }

        UniTask RateMeAsync(CancellationToken cancellationToken);
    }
}