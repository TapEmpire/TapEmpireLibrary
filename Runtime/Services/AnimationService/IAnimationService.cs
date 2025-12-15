using System;
using DG.Tweening;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IAnimationService<ResourceType> : IService
    {
        public Sequence CollectResource(ResourceType resourceType, int amount, Vector3 start, bool shouldAddResource);

        public Sequence CollectVirtualResource(int amount, Vector3 start, Transform target, float scatterRadius, float scatterRandomness, Vector2 sizeDelta, Sprite sprite, Action onComplete);
    }
}
