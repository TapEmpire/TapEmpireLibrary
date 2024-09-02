using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IInitializable
    {
        bool Initialized { get; }

        int Order { get; set; }

        UniTask InitializeAsync(CancellationToken cancellationToken);

        void Release();
    }
}