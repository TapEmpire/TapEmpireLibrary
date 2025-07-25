using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.UI;

namespace TapEmpire.Services
{
    public interface ISceneManagementService : IService
    {
        UniTask CreateLoadingScreen(CancellationToken cancellationToken, bool isLoadingVisible = true);
        UniTask CloseLoadingScreen(CancellationToken cancellationToken);
        UniTask LoadSceneAsync(SceneName sceneName, CancellationToken cancellationToken, bool manualLoadingClose = false, bool isLoadingVisible = true);
        void ChangeLoadingScreen(SceneLoadingUIView loadingUIView);
    }
}