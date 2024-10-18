using System;
using System.Linq;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [CreateAssetMenu(fileName = nameof(ServicesInstaller), menuName = "TapEmpire/Installers/" + nameof(ServicesInstaller))]
    public class ServicesInstaller : ScriptableObjectInstaller<ServicesInstaller>
    {
        [SerializeReference]
        private IService[] _orderedServices = Array.Empty<IService>();
        
        [SerializeReference]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private IService[] _services = Array.Empty<IService>();

        private ServicesContainer _servicesContainer;

        [NonSerialized]
        private IDisposable _subscription;

        public override void InstallBindings()
        {
            _servicesContainer = new ServicesContainer(Container);
            Container.Bind<ServicesContainer>().FromInstance(_servicesContainer).AsSingle();
            
            _orderedServices.ForEachIndexed(ConfigureService);
            _services.ForEach(service => ConfigureService(service, -1));
        }
        
        private void ConfigureService(IService service, int order)
        {
            service.Order = order;
            if (service.Initialized)
            {
                service.Release();
            }
            var serviceType = service.GetType();
            var serviceInterfaces = serviceType
                .GetInterfaces()
                .Where(i => i != typeof(IService) && typeof(IService)
                    .IsAssignableFrom(i));

            foreach (var serviceInterface in serviceInterfaces)
            {
                Container.Bind(serviceInterface).FromInstance(service).AsSingle();
            }
            _servicesContainer.AddToRuntimeList(service);
        }
    }
}