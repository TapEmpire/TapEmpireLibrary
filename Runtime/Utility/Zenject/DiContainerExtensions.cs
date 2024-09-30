using System;
using UnityEngine;
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
                InitializeTicks(tickableManager, target);
            }
        }
        
        public static void InitializeTicks<T>(this TickableManager tickableManager, T target) where T : class
        {
            try
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
            // я сам не люблю try catch, но тут ошибка внутри zenject. Он видимо внутри создает таски в разных потоках с приоритетами.
            // и если быстро нажимать рестарт (удалять-добавлять модули), то выдает ошибку дублирования. не вижу как это можно обойти
            // без того чтобы лезть в код zenject дальше
            
            // ! причем с этой ошибкой в работе ничего не ломается, игра норм играется !
            catch (Exception e)
            {
                Debug.LogWarning($"InitializeTicks error {e.Message}");
            }
        }
        
        public static void ReleaseTicks<T>(this DiContainer diContainer, params T[] targets) where T : class
        {
            var tickableManager = diContainer.Resolve<TickableManager>();

            foreach (var target in targets)
            {
                ReleaseTicks(tickableManager, target);
            }
        }
        
        public static void ReleaseTicks<T>(this TickableManager tickableManager, T target) where T : class
        {
            try
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
            catch (Exception e)
            {
                Debug.LogWarning($"ReleaseTicks error {e.Message}");
            }
        }
    }
}