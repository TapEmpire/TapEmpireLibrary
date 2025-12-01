using DG.Tweening;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IAnimationService<ResourceType> : IService
    {
        public Sequence CollectResource(ResourceType resourceType, int amount, Vector3 start, bool shouldAddResource);
    }
}
