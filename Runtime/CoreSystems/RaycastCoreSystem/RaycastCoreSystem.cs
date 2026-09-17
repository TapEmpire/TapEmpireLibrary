using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class RaycastCoreSystem : Initializable, IRaycastCoreSystem
    {
        private const int MaxOverlapResults = 10;
        private const int MaxCircleOverlapResults = 32;

        [SerializeField] private LayerMask _raycastLayers = default;
        [SerializeField] private LayerMask _overlapLayers = default;

        public Vector2 InputWorldPoint
        {
            get
            {
                DropStaleCache();
                _inputWorldPoint ??= (Vector2)_camera.ScreenToWorldPoint(_inputCoreSystem.InputPosition);
                return _inputWorldPoint.Value;
            }
        }

        public RaycastHit2D RaycastHit2D
        {
            get
            {
                DropStaleCache();
                _raycastHit2D ??= DoRaycast(_raycastLayers);
                return _raycastHit2D.Value;
            }
        }

        private IInputCoreSystem _inputCoreSystem;
        private ILevelExecutionCoreSystem _levelExecutionCoreSystem;

        private Vector2? _inputWorldPoint;
        private RaycastHit2D? _raycastHit2D;
        private int _cachedFrame = -1;
        private Camera _camera;
        private IDisposable _subscription;
        private ContactFilter2D _overlapFilter;
        private RaycastHit2D[] _raycastResults = new RaycastHit2D[1];
        private Collider2D[] _overlapResults = new Collider2D[MaxOverlapResults];
        private Collider2D[] _circleOverlapResults = new Collider2D[MaxCircleOverlapResults];

        [Inject]
        private void Construct(IInputCoreSystem inputCoreSystem, ILevelExecutionCoreSystem levelExecutionCoreSystem)
        {
            _inputCoreSystem = inputCoreSystem;
            _levelExecutionCoreSystem = levelExecutionCoreSystem;

            _overlapFilter = CreateFilter(_overlapLayers, true);
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _subscription = _levelExecutionCoreSystem.ExecutionData.Subscribe(OnUpdateExecutionData);
            return base.OnInitializeAsync(cancellationToken);
        }

        private void OnUpdateExecutionData(LevelExecutionData levelExecutionData)
        {
            _camera = levelExecutionData?.LevelView.GetLevelReference<Camera>("Camera");
        }

        protected override void OnRelease()
        {
            _subscription.Dispose();
        }

        public RaycastHit2D RaycastHit2DLayered(LayerMask layerMask)
        {
            return DoRaycast(layerMask);
        }

        public Vector2 WorldPoint(Vector2 screenPosition)
        {
            return _camera.ScreenToWorldPoint(screenPosition);
        }

        public RaycastHit2D Raycast(Vector2 worldPoint)
        {
            return Raycast(worldPoint, _raycastLayers);
        }

        public RaycastHit2D Raycast(Vector2 worldPoint, LayerMask layerMask)
        {
            var hitCount = Physics2D.Raycast(worldPoint, Vector2.zero, CreateFilter(layerMask, false), _raycastResults, float.MaxValue);
            return hitCount > 0 ? _raycastResults[0] : default;
        }

        public Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB)
        {
            return Physics2D.OverlapAreaAll(pointA, pointB, _overlapLayers);
        }

        // The result is a view over a shared buffer, not a copy: the next Overlap or OverlapTouching
        // call overwrites it. Finish iterating before calling either of them again.
        public ArraySegment<Collider2D> Overlap(Collider2D target)
        {
            var count = target.Overlap(_overlapFilter, _overlapResults);
            return new ArraySegment<Collider2D>(_overlapResults, 0, count);
        }

        public ArraySegment<Collider2D> OverlapTouching(Collider2D target)
        {
            var count = target.Overlap(_overlapFilter, _overlapResults);
            var touching = 0;

            for (var i = 0; i < count; i++)
            {
                if (target.IsTouching(_overlapResults[i], _overlapFilter))
                {
                    _overlapResults[touching++] = _overlapResults[i];
                }
            }

            return new ArraySegment<Collider2D>(_overlapResults, 0, touching);
        }

        public ArraySegment<Collider2D> OverlapCircle(Vector2 point, float radius)
        {
            var count = Physics2D.OverlapCircle(point, radius, _overlapFilter, _circleOverlapResults);
            return new ArraySegment<Collider2D>(_circleOverlapResults, 0, count);
        }

        private RaycastHit2D DoRaycast(LayerMask layerMask)
        {
            return Raycast(InputWorldPoint, layerMask);
        }

        private static ContactFilter2D CreateFilter(LayerMask layerMask, bool useTriggers)
        {
            return new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = useTriggers,
                layerMask = layerMask,
            };
        }

        private void DropStaleCache()
        {
            if (_cachedFrame == Time.frameCount) return;

            _cachedFrame = Time.frameCount;
            _inputWorldPoint = null;
            _raycastHit2D = null;
        }
    }
}
