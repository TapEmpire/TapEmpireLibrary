using UnityEngine;
using Zenject;

namespace TapEmpire.Utility
{
    public static class TicksContainerExtensions
    {
        public static void Initialize(this ITicksContainer self, DiContainer diContainer)
        {
            var tickableManager = diContainer.TryResolve<TickableManager>();
            if (tickableManager == null)
            {
                Debug.LogError("Cant resolve tickableManager");
                return;
            }
            self.Initialize(tickableManager);
        }
    }
}