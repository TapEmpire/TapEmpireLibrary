using Zenject;

namespace TapEmpire.Utility
{
    public interface ITicksContainer
    {
        bool Initialized { get; }
        
        void TryInitialize(TickableManager tickableManager);

        void TryRelease();

        void TryAddTicks<T>(T target);

        void TryRemoveTicks<T>(T target);
    }
}