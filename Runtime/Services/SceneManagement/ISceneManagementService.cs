using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface ISceneManagementService : IService
    {
        UniTask CreateLoadingScreen(CancellationToken cancellationToken);
        UniTask CloseLoadingScreen(CancellationToken cancellationToken);
        UniTask LoadSceneAsync(SceneName sceneName, CancellationToken cancellationToken);
    }
}