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

        public override void InstallBindings()
        {
            foreach (var system in _systems)
            {
                var systemType = system.GetType();
                var systemInterfaces = systemType.GetInterfaces()
                    .Where(i => i != typeof(ICoreSystem) && typeof(ICoreSystem).IsAssignableFrom(i));

                foreach (var systemInterface in systemInterfaces)
                {
                    Container.Bind(systemInterface).FromInstance(system);
                }
                /*if (typeof(ITickable).IsAssignableFrom(systemType))
                {
                    Container.Bind<ITickable>().To(systemType).FromInstance(system);
                }
                if (typeof(IFixedTickable).IsAssignableFrom(systemType))
                {
                    Container.Bind<IFixedTickable>().To(systemType).FromInstance(system);
                }
                if (typeof(ILateTickable).IsAssignableFrom(systemType))
                {
                    Container.Bind<ILateTickable>().To(systemType).FromInstance(system);
                }*/
            }

            Container.Bind<ICoreSystem[]>().FromInstance(_systems);
        }
    }
}