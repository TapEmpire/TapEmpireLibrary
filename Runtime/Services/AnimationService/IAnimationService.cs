using System;
using DG.Tweening;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IAnimationService<ResourceType> : IService
    {
        public Sequence CollectResource(ResourceType resourceType, int amount, Vector3 start, bool shouldAddResource);

        public Sequence CollectVirtualResource(int amount, Vector3 start, Transform target, float scatterRadius, float scatterRandomness, Vector2 sizeDelta, Sprite sprite, Action onComplete);

        public Sequence PlayFlyingPrefabAnimation(int count, Vector3 start, Transform target, float spawnInterval, float scatterRadius, float scatterRandomness, GameObject prefab, Action<int> onItemComplete, Action onAllComplete = null);

        public Sequence FloatUpResource(ResourceType resourceType, int amount, Transform target, Vector3 start, bool shouldAddResource);

        public Sequence FloatUp(Transform item, Transform target, Vector3 start);
    }
}
