using Zenject;

namespace TapEmpire.Utility
{
    public interface ITicksContainer
    {
        bool TryAddToTickableManager(TickableManager tickableManager);

        bool TryRemoveFromTickableManager();
        
        void TryAddTicks<T>(T[] targets) where T : class;
        
        void TryAddTicks<T>(T target) where T : class;

        void TryRemoveTicks<T>(T target) where T : class;
        
        void TryRemoveTicks<T>(T[] targets) where T : class;

    }
}