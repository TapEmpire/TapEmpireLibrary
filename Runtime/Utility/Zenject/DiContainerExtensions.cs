using Zenject;

namespace TapEmpire.Utility
{
    public static class DiContainerExtensions
    {
        public static void InitializeTicks<T>(this DiContainer diContainer, params T[] targets) where T : class
        {
            var tickableManager = diContainer.Resolve<TickableManager>();
            foreach (var target in targets)
            {
                if (target is ITickable tickable)
                {
                    tickableManager.Add(tickable);
                }
                if (target is IFixedTickable fixedTickable)
                {
                    tickableManager.AddFixed(fixedTickable);
                }
                if (target is ILateTickable lateTickable)
                {
                    tickableManager.AddLate(lateTickable);
                }
            }
        }
        
        public static void ReleaseTicks<T>(this DiContainer diContainer, params T[] targets) where T : class
        {
            var tickableManager = diContainer.Resolve<TickableManager>();

            foreach (var target in targets)
            {
                if (target is ITickable tickable)
                {
                    tickableManager.Remove(tickable);
                }
                if (target is IFixedTickable fixedTickable)
                {
                    tickableManager.RemoveFixed(fixedTickable);
                }
                if (target is ILateTickable lateTickable)
                {
                    tickableManager.RemoveLate(lateTickable);
                }
            }
        }
    }
}