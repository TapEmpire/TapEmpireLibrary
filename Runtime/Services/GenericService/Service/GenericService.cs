using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;
using R3;
using TapEmpire.Modules;
using System;

namespace TapEmpire.Services.Generic
{
    [System.Serializable]
    public class GenericService : Initializable, IGenericService
    {
        private DiContainer _diContainer;

        [SerializeReference] private IGenericServiceModule[] _modules;
        private Dictionary<Type, IGenericServiceModule> _liveModules = null;

        [Inject]
        private void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _liveModules = new();
            var moduleInitializations = _modules.Select(module => InitializeAndRegisterModuleAsync(module, cancellationToken));
            await UniTask.WhenAll(moduleInitializations);

            await base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _liveModules?.ForEach(pair => pair.Value.Dispose());
            _liveModules?.Clear();

            base.OnRelease();
        }

        public T GetServiceModule<T>() where T : class, IGenericServiceModule => _liveModules[typeof(T)] as T;

        private async UniTask InitializeAndRegisterModuleAsync(IGenericServiceModule module, CancellationToken cancellationToken)
        {
            await module.InitializeAsync(_diContainer, cancellationToken);
            _liveModules[module.GetType()] = module;
        }
    }
}
