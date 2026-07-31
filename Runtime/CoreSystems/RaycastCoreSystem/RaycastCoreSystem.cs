using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class RaycastCoreSystem : Initializable, IRaycastCoreSystem, ITickable
    {
        private const int MaxOverlapResults = 10;

        [SerializeField] private LayerMask _layers = default;

        public Vector2 InputWorldPoint
        {
            get
            {
                _inputWorldPoint ??= (Vector2)_camera.ScreenToWorldPoint(_inputCoreSystem.InputPosition);
                return _inputWorldPoint.Value;
            }
        }

        public RaycastHit2D RaycastHit2D
        {
            get
            {
                _raycastHit2D ??= DoRaycast(_layers);
                return _raycastHit2D.Value;
            }
        }

        private IInputCoreSystem _inputCoreSystem;
        private ILevelExecutionCoreSystem _levelExecutionCoreSystem;

        private Vector2? _inputWorldPoint;
        private RaycastHit2D? _raycastHit2D;
        private Camera _camera;
        private IDisposable _subscription;
        private ContactFilter2D _overlapFilter;
        private RaycastHit2D[] _raycastResults = new RaycastHit2D[1];

        [Inject]
        private void Construct(IInputCoreSystem inputCoreSystem, ILevelExecutionCoreSystem levelExecutionCoreSystem)
        {
            _inputCoreSystem = inputCoreSystem;
            _levelExecutionCoreSystem = levelExecutionCoreSystem;

            _overlapFilter = CreateFilter(_layers, true);
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

        public Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB)
        {
            return Physics2D.OverlapAreaAll(pointA, pointB, _layers);
        }

        public Collider2D[] Overlap(Collider2D target)
        {
            var hits = new Collider2D[MaxOverlapResults];
            var count = target.Overlap(_overlapFilter, hits);

            return hits.Take(count).Where(collider => target.IsTouching(collider, _overlapFilter)).ToArray();
        }

        private RaycastHit2D DoRaycast(LayerMask layerMask)
        {
            if (_camera == null) return default;

            var hitCount = Physics2D.Raycast(InputWorldPoint, Vector2.zero, CreateFilter(layerMask, false), _raycastResults, float.MaxValue);

            return hitCount > 0 ? _raycastResults[0] : default;
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

        public void Tick()
        {
            _inputWorldPoint = null;
            _raycastHit2D = null;
        }
    }
}
