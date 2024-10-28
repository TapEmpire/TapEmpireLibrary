using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    public class CoreSystemsInstaller : MonoInstaller<CoreSystemsInstaller>
    {
        [SerializeReference]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private ICoreSystem[] _systems = Array.Empty<ICoreSystem>();

        [NonSerialized]
        private IDisposable _subscription;

        private CoreSystemsContainer _coreSystemsContainer;

        public override void InstallBindings()
        {
            _coreSystemsContainer = new CoreSystemsContainer(Container);
            Container.Bind<CoreSystemsContainer>().FromInstance(_coreSystemsContainer).AsSingle();
            
            foreach (var system in _systems)
            {
                var systemType = system.GetType();
                var systemInterfaces = systemType.GetInterfaces()
                    .Where(i => i != typeof(ICoreSystem) && typeof(ICoreSystem).IsAssignableFrom(i));

                foreach (var systemInterface in systemInterfaces)
                {
                    Container.Bind(systemInterface).FromInstance(system);
                }
                _coreSystemsContainer.AddToRuntimeList(system);
            }
        }
    }
}