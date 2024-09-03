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
                actionOnGet: (item) => item.gameObject.SetActive(true),
                actionOnRelease: (item) => item.gameObject.SetActive(false),
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