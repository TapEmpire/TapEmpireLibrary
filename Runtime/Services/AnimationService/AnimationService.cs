using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;
using TapEmpire.Fragments;
using Zenject;
using R3;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class AnimationService<ResourceType> : Initializable, IAnimationService<ResourceType>
    {
        [SerializeField] private AnimationSettings _settings;
        [SerializeField] private Image _flyingResourcePrefab;

        private IUIService _uiService;
        private IResourcesService<ResourceType> _resourcesService;
        protected IAudioService _audioService;

        private ComponentPool<Image> _flyingResources;
        private Transform _parent;
        private const int MaxResourceAmount = 40;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IUIService uiService, IResourcesService<ResourceType> resourceService, IAudioService audioService)
        {
            _uiService = uiService;
            _resourcesService = resourceService;
            _audioService = audioService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            CreatePoolParent();
            CreateFlyingResourcesPool();
            _resourcesService.OnVirtualAdded.Subscribe(OnVirtualAdded).AddTo(_disposables);
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _flyingResources.Clear();
            base.OnRelease();
        }

        public Sequence CollectResource(ResourceType resourceType, int amount, Vector3 start, bool shouldAddResource)
        {
            var newAmount = Mathf.Clamp(amount, 0, MaxResourceAmount);
            var resourceAmount = amount / newAmount;
            var firstResourceAmount = amount - resourceAmount * (newAmount - 1);

            var sprite = _resourcesService.GetFlyingSprite(resourceType);
            var target = _uiService.ShibariContext.TryGetValue($"{resourceType}Hud");

            if (target == null)
            {
                if (shouldAddResource)
                {
                    _resourcesService.Add(resourceType, amount);
                }
                return DOTween.Sequence();
            }

            var points = AnimationFragment.GetRadialSpreadPoints(start, newAmount, _settings.ScatterRadius, _settings.ScatterRandomness);
            var animation = DOTween.Sequence();
            var end = target.position;
            var index = 0;

            foreach (var point in points)
            {
                var flyAmount = index++ > 0 ? resourceAmount : firstResourceAmount;
                var resourceRenderer = _flyingResources.Get();
                var resource = resourceRenderer.transform;
                resourceRenderer.sprite = sprite;

                resource.position = start;
                resource.parent = target.transform;

                var sequence = DOTween.Sequence();
                resource.DOMove(point, 0.3f).AppendTo(sequence);
                resource.DOMove(end, 0.5f).SetDelay(Random.Range(0.05f, 0.2f)).SetEase(Ease.InBack).AppendTo(sequence);
                sequence.AppendCallback(() =>
                {
                    if (shouldAddResource)
                    {
                        _resourcesService.Add(resourceType, flyAmount);
                    }
                    _flyingResources.Release(resourceRenderer);
                    resourceRenderer.transform.parent = _parent;
                });
                animation.Join(sequence);
            }

            animation.SetLink(target.gameObject);

            return animation;
        }

        private void CreatePoolParent()
        {
            var parent = new GameObject("PoolParent");
            _parent = parent.transform;
            Object.DontDestroyOnLoad(parent);
        }

        private void CreateFlyingResourcesPool()
        {
            _flyingResources = new ComponentPool<Image>(_flyingResourcePrefab, _parent, MaxResourceAmount, MaxResourceAmount);
        }

        protected virtual void OnVirtualAdded(ResourceType type) { }
    }
}
