using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace TapEmpire.Services
{
    public static class InitializableUtility
    {
        public static UniTask InitializeAsync<T>(T[] initializables, DiContainer diContainer, CancellationToken cancellationToken) where T : IInitializable
        {
            if (initializables.All(initializable => initializable.Initialized))
            {
                return UniTask.CompletedTask;
            }
            foreach (var initializable in initializables)
            {
                diContainer.Inject(initializable);
            }
            var tasks = Enumerable.Select(initializables, initializable => initializable.InitializeAsync(cancellationToken));
            return UniTask.WhenAll(tasks);
        }

        public static UniTask WaitUntilAllInitializedAsync<T>(T[] initializables, CancellationToken cancellationToken) where T : IInitializable
        {
            if (initializables.All(initializable => initializable.Initialized))
            {
                return UniTask.CompletedTask;
            }
            return UniTask.WaitUntil(() => initializables.All(initializable => initializable.Initialized), cancellationToken: cancellationToken);
        }
    }
}