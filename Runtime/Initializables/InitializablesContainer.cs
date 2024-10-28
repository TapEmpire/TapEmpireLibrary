using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Utility;
using TapEmpireLibrary.Game;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    public abstract class InitializablesContainer<T> where T : IInitializable
    {
        private readonly List<T> _runtimeList = new();
        
        private readonly DiContainer _diContainer;
        private IGameEventsContainer _gameEventsContainer;
        private ITicksContainer _ticksContainer;

        private bool _initialized;

        protected InitializablesContainer(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public void AddToRuntimeList(T item)
        {
            if (!_runtimeList.Contains(item))
            {
                _runtimeList.Add(item);
            }
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_initialized)
            {
                Debug.Log($"Container of {typeof(T)} already initialized");
                return;
            }
            _gameEventsContainer = _diContainer.TryResolve<IGameEventsContainer>();
            if (_gameEventsContainer == null)
            {
                Debug.LogError($"Error resolving {nameof(IGameEventsContainer)}, bind it before initializing {typeof(T)}");
                return;
            }
            _ticksContainer = _diContainer.TryResolve<ITicksContainer>();
            if (_ticksContainer == null)
            {
                Debug.LogError($"Error resolving {nameof(ITicksContainer)}, bind it before initializing {typeof(T)}");
                return;
            }
            if (!_ticksContainer.Initialized)
            {
                var tickableManager = _diContainer.TryResolve<TickableManager>();
                if (tickableManager == null)
                {
                    Debug.LogError($"Error resolving TickableManager");
                    return;
                }
                _ticksContainer.TryInitialize(tickableManager);
            }
            
            _gameEventsContainer.OnApplicationQuitEvent += GameEventsContainer_OnApplicationQuitEvent;
            
            var initializablesArray = _runtimeList.ToArray();
            await InitializeAsync(initializablesArray, cancellationToken);

            foreach (var initializable in _runtimeList)
            {
                _ticksContainer.TryAddTicks(initializable);
            }

            _initialized = true;
        }

        private void GameEventsContainer_OnApplicationQuitEvent()
        {
            Release();
        }

        // пока оставил публичный метод, но вообще по идее он сам себя релизит
        public void Release()
        {
            foreach (var initializable in _runtimeList)
            {
                _ticksContainer.TryRemoveTicks(initializable);
                initializable.Release();
            }
            _runtimeList.Clear();
            
            _ticksContainer.TryRelease();
            
            _gameEventsContainer.OnApplicationQuitEvent -= GameEventsContainer_OnApplicationQuitEvent;
            
            _initialized = false;
        }

        private async UniTask InitializeAsync(T[] initializables, CancellationToken cancellationToken)
        {
            if (initializables.All(initializable => initializable.Initialized))
            {
                return;
            }
            foreach (var initializable in initializables)
            {
                _diContainer.Inject(initializable);
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

        // TODO вроде не используется, убрать
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