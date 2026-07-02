using System.Collections.Generic;
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

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _liveModules = new();
            Array.ForEach(_modules, module => InitializeAndRegisterModule(module));

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _liveModules?.ForEach(pair => pair.Value.Dispose());
            _liveModules?.Clear();

            base.OnRelease();
        }

        public T GetServiceModule<T>() where T : class, IGenericServiceModule => _liveModules[typeof(T)] as T;

        private void InitializeAndRegisterModule(IGenericServiceModule module)
        {
            module.Initialize(_diContainer);
            _liveModules[module.GetType()] = module;
        }
    }
}
