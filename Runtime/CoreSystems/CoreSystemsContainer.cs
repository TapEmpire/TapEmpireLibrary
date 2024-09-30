using TapEmpire.Services;
using Zenject;

namespace TapEmpire.CoreSystems
{
    public class CoreSystemsContainer : InitializablesContainer<ICoreSystem>
    {
        public CoreSystemsContainer(DiContainer diContainer) : base(diContainer)
        {
        }
    }
}