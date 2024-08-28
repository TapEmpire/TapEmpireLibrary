using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IInitializable
    {
        bool Initialized { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken);

        void Release();
    }
}