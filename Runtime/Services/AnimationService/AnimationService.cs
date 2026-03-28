using System;
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
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TapEmpire.Services
{
    [Serializable]
    public class AnimationService<ResourceType> : Initializable, IAnimationService<ResourceType>
    {
        [SerializeField] private AnimationSettings _settings;
        [SerializeField] private Image _flyingResourcePrefab;

        private IUIService _uiService;
        private IResourcesService<ResourceType> _resourcesService;
        protected IAudioService _audioService;

        private ComponentPool<Image> _flyingResources;
        private ComponentPool<Transform> _flyingPrefabPool;
        private GameObject _prefabTemplate;
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
            _flyingResources?.Clear();
            _flyingPrefabPool?.Clear();
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

            return PlayFlyingAnimation(
                newAmount,
                start,
                target,
                _settings.ScatterRadius,
                _settings.ScatterRandomness,
                sprite,
                configureRenderer: null,
                onItemComplete: i =>
                {
                    if (shouldAddResource)
                    {
                        var flyAmount = i == 1 ? firstResourceAmount : resourceAmount;
                        _resourcesService.Add(resourceType, flyAmount);
                    }
                });
        }

        public Sequence CollectVirtualResource(
            int amount,
            Vector3 start,
            Transform target,
            float scatterRadius,
            float scatterRandomness,
            Vector2 sizeDelta,
            Sprite sprite,
            Action onComplete)
        {
            var newAmount = Mathf.Clamp(amount, 0, MaxResourceAmount);
            
            return PlayFlyingAnimation(
                newAmount,
                start,
                target,
                scatterRadius,
                scatterRandomness,
                sprite,
                configureRenderer: image =>
                {
                    image.transform.localScale = Vector3.one;
                    image.rectTransform.sizeDelta = sizeDelta;
                    image.raycastTarget = false;
                },
                onItemComplete: _ =>
                {
                    onComplete?.Invoke();
                });
        }

        private Sequence PlayFlyingAnimation(
            int count,
            Vector3 start,
            Transform target,
            float scatterRadius,
            float scatterRandomness,
            Sprite sprite,
            Action<Image> configureRenderer,
            Action<int> onItemComplete)
        {
            var newCount = Mathf.Clamp(count, 0, MaxResourceAmount);
            var points = AnimationFragment.GetRadialSpreadPoints(start, newCount, scatterRadius, scatterRandomness);
            var animation = DOTween.Sequence();
            var end = target.position;
            var index = 0;

            foreach (var point in points)
            {
                var resourceRenderer = _flyingResources.Get();
                
                resourceRenderer.sprite = sprite;  
                
                configureRenderer?.Invoke(resourceRenderer);

                var resource = resourceRenderer.transform;
                resource.position = start;
                resource.parent = target;

                var sequence = DOTween.Sequence();
                resource.DOMove(point, 0.3f).AppendTo(sequence);
                resource.DOMove(end, 0.5f)
                    .SetDelay(Random.Range(0.05f, 0.2f))
                    .SetEase(Ease.InBack)
                    .AppendTo(sequence);

                var flyAmount = ++index;

                sequence.AppendCallback(() =>
                {
                    onItemComplete?.Invoke(flyAmount);
                    _flyingResources.Release(resourceRenderer);
                    resourceRenderer.transform.parent = _parent;
                });

                animation.Join(sequence);
            }

            animation.SetLink(target.gameObject);
            return animation;
        }

        public Sequence PlayFlyingPrefabAnimation(
            int count,
            Vector3 start,
            Transform target,
            float spawnInterval,
            float scatterRadius,
            float scatterRandomness,
            GameObject prefab,
            Action<int> onItemComplete,
            Action onAllComplete = null)
        {
            if (!target || !prefab)
                return DOTween.Sequence();

            CreatePrefabPool(prefab);

            const float birthDuration = 0.2f;
            const float hoverDuration = 0.15f;
            const float flyDuration = 0.5f;
            const float hoverHeight = 20f;

            var newCount = Mathf.Clamp(count, 1, MaxResourceAmount);
            var spawnPoints = AnimationFragment.GetRadialSpreadPoints(start, newCount, scatterRadius, scatterRandomness);
            var animation = DOTween.Sequence();
            var end = target.position;

            for (var i = 0; i < newCount; i++)
            {
                var resourceTransform = _flyingPrefabPool.Get();
                resourceTransform.localScale = Vector3.zero;

                var spawnPos = (Vector3)spawnPoints[i];
                var hoverPos = spawnPos + Vector3.up * hoverHeight;
                resourceTransform.position = spawnPos;
                resourceTransform.SetParent(target);

                var sequence = DOTween.Sequence();
                sequence.SetDelay(i * spawnInterval);
                sequence.Append(resourceTransform.DOScale(1f, birthDuration).SetEase(Ease.OutBack));
                sequence.Append(resourceTransform.DOMove(hoverPos, hoverDuration).SetEase(Ease.OutQuad));
                sequence.Append(resourceTransform.DOMove(end, flyDuration).SetEase(Ease.InBack));

                var flyIndex = i + 1;
                var isLast = flyIndex == newCount;

                sequence.AppendCallback(() =>
                {
                    onItemComplete?.Invoke(flyIndex);
                    _flyingPrefabPool.Release(resourceTransform);
                    resourceTransform.SetParent(_parent);

                    if (isLast)
                        onAllComplete?.Invoke();
                });

                animation.Join(sequence);
            }

            animation.SetLink(target.gameObject);
            return animation;
        }

        private void CreatePrefabPool(GameObject prefab)
        {
            if (_prefabTemplate == prefab && _flyingPrefabPool != null)
                return;

            _flyingPrefabPool?.Clear();
            _prefabTemplate = prefab;
            var prefabTransform = prefab.GetComponent<Transform>();
            _flyingPrefabPool = new ComponentPool<Transform>(prefabTransform, _parent, MaxResourceAmount, MaxResourceAmount);
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
