using Zenject;

namespace TapEmpire.Services
{
    public class ServicesContainer : InitializablesContainer<IService>
    {
        public ServicesContainer(DiContainer diContainer) : base(diContainer)
        {
        }
    }
}