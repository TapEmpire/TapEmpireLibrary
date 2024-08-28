using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [CreateAssetMenu(fileName = nameof(ServicesInstaller), menuName = "TapEmpire/Installers/" + nameof(ServicesInstaller))]
    public class ServicesInstaller : ScriptableObjectInstaller<ServicesInstaller>
    {
        [NonSerialized]
        private readonly List<IService> _runtimeServices = new();
        
        [SerializeReference]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private IService[] _services = Array.Empty<IService>();

        [NonSerialized]
        private IDisposable _subscription;

        public override void InstallBindings()
        {
            _runtimeServices.Clear();
            foreach (var service in _services)
            {
                ConfigureService(service);
            }
            Container.Bind<IService[]>().FromInstance(_runtimeServices.ToArray()).AsSingle();
            
            SubscribeToApplicationExit(Application.exitCancellationToken);
        }
        
        private void ConfigureService(IService service)
        {
            if (service.Initialized)
            {
                service.Release();
            }
            var serviceType = service.GetType();
            _runtimeServices.Add(service);
            var serviceInterfaces = serviceType
                .GetInterfaces()
                .Where(i => i != typeof(IService) && typeof(IService)
                    .IsAssignableFrom(i));

            foreach (var serviceInterface in serviceInterfaces)
            {
                Container.Bind(serviceInterface).FromInstance(service).AsSingle();
            }
            if (typeof(ITickable).IsAssignableFrom(serviceType))
            {
                Container.Bind<ITickable>().To(serviceType).FromInstance(service).AsSingle();
            }
        }

        private void SubscribeToApplicationExit(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                foreach (var service in _runtimeServices)
                {
                    if (service.Initialized)
                    {
                        service.Release();
                    }
                }
            });
        }
    }
}