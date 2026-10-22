using Zenject;

namespace TapEmpire.Utility
{
    public interface ITicksContainer
    {
        bool Initialized { get; }

        bool IsPaused { get; set; }

        void TryInitialize(TickableManager tickableManager);

        void TryRelease();

        void TryAddTicks<T>(T target);

        void TryRemoveTicks<T>(T target);
    }
}