
namespace TapEmpire.CoreSystems
{
    public interface IExecutionCoreSystem : ICoreSystem
    {
        T GetModule<T>() where T : IExecutionModule;
    }
}
