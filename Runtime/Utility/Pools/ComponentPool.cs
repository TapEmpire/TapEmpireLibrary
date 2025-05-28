using UnityEngine;
using UnityEngine.Pool;

namespace TapEmpire.Utility
{
    public class ComponentPool<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public ComponentPool(T prefab, Transform defaultParent = null, int defaultCapacity = 10, int maxSize = 20)
        {
            var hasDefaultParent = defaultParent != null;
            _pool = new ObjectPool<T>(
                createFunc: () => hasDefaultParent ? Object.Instantiate(prefab, defaultParent) : Object.Instantiate(prefab),
                actionOnGet: (item) => Utility.SetGameObjectActive(item, true),
                actionOnRelease: (item) => Utility.SetGameObjectActive(item, false),
                actionOnDestroy: (item) =>
                {
                    if (item != null)
                    {
                        Object.Destroy(item.gameObject);
                    }
                },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        public T Get()
        {
            return _pool.Get();
        }

        public T GetSafe()
        {
            try
            {
                return _pool.Get();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"{e.Message} {e.StackTrace}");
            }

            return null;
        }

        public void Release(T item)
        {
            _pool.Release(item);
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}