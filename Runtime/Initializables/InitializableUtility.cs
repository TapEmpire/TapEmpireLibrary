using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services
{
    public static class InitializableUtility
    {
        // TODO remove - deprecated, use InitializablesContainer instead
        public static async UniTask InitializeAsync<T>(T[] initializables, DiContainer diContainer, CancellationToken cancellationToken) where T : IInitializable
        {
            if (initializables.All(initializable => initializable.Initialized))
            {
                return; // UniTask.CompletedTask;
            }
            foreach (var initializable in initializables)
            {
                diContainer.Inject(initializable);
            }

            // TODO: Replace with [Required] token
            var (ordered, unordered) = initializables.Partition(initializable => initializable.Order >= 0);

            var sorted = ordered.OrderBy(x => x.Order);
            foreach (var initializable in sorted)
            {
                await initializable.InitializeAsync(cancellationToken);
            }

            // TODO: end

            var tasks = Enumerable.Select(unordered, initializable => initializable.InitializeAsync(cancellationToken));
            await UniTask.WhenAll(tasks);
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