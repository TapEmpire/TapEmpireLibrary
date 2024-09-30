using Zenject;

namespace TapEmpire.Utility
{
    public interface ITicksContainer
    {
        bool Initialized { get; }
        
        void Initialize(TickableManager tickableManager);

        void Release();

        void TryAddTicks<T>(T target);

        void TryRemoveTicks<T>(T target);
    }
}