using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface ISceneManagementService : IService
    {
        UniTask LoadSceneAsync(SceneName sceneName, CancellationToken cancellationToken);
    }
}