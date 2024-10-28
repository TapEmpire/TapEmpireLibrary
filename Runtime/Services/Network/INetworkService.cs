using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface INetworkService : IService
    {
        bool HasConnection { get; }

        UniTask WaitNetworkAsync(CancellationToken cancellationToken, bool withUI = true);
    }
}