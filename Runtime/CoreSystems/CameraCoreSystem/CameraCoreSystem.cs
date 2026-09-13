using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Level;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class CameraCoreSystem : Initializable, ICameraCoreSystem
    {
        [SerializeField] private float _bottomPadding = 0.0f;

        private ILevelExecutionCoreSystem _levelExecutionCoreSystem;
        private IAdsService _adsService;

        private const float BannerHeight = 175.0f;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(ILevelExecutionCoreSystem levelExecutionCoreSystem, IAdsService adsService)
        {
            _levelExecutionCoreSystem = levelExecutionCoreSystem;
            _adsService = adsService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _levelExecutionCoreSystem.ExecutionData.Subscribe(_ => Adjust()).AddTo(_disposables);
            _adsService.AdsEnabled.Subscribe(_ => Adjust()).AddTo(_disposables);
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
        }

        public void Adjust()
        {
            var executionData = _levelExecutionCoreSystem.ExecutionData.Value;
            if (executionData == null) return;

            var levelView = executionData.LevelView;
            var camera = levelView.Camera;
            var bounds = levelView.GetLevelReference<BoundsProvider>("Geometry").Bounds;

            var aspect = (float)Screen.width / Screen.height;
            var orthographicSize = bounds.size.x * 0.5f / aspect;

            var bottomInset = _adsService.AdsEnabled.CurrentValue
                ? _bottomPadding + ToWorldHeight(levelView, BannerHeight, orthographicSize)
                : _bottomPadding;

            camera.orthographicSize = orthographicSize;
            camera.transform.position = new Vector3(
                bounds.center.x,
                bounds.min.y - bottomInset + orthographicSize,
                camera.transform.position.z);
        }

        private static float ToWorldHeight(LevelView levelView, float referencePixels, float orthographicSize)
        {
            var canvas = levelView.GetLevelReference<Canvas>("Canvas");
            return referencePixels * canvas.scaleFactor / Screen.height * orthographicSize * 2.0f;
        }
    }
}
