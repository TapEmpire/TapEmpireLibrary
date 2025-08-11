using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TapEmpire.Services;
using UnityEngine;
using WordGame.CoreSystems;
using Zenject;

namespace TapEmpire.UI
{
    public class CustomCursorUIView : UIView<CustomCursorUIViewModel>, IInjectable
    {
        [SerializeField] private RectTransform _imageTransform;
        [SerializeField] private GameObject _imageDefault;
        [SerializeField] private GameObject _imagePressed;

        private DiContainer _diContainer;
        private ISceneContextsService _sceneContextsService;
        private IInputCoreSystem _inputCoreSystem;
        private ILevelExecutionCoreSystem _levelExecutionCoreSystem;

        private RectTransform _canvasTransform;
        private bool _isRunning;
        private bool _isSimulating;

        private CompositeDisposable _compositeDisposable;

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            var canvas = GetComponentInParent<Canvas>();
            _canvasTransform = (RectTransform) canvas.transform;

            _isRunning = true;

            return base.OpenAsync(cancellationToken);
        }

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;

            _compositeDisposable = new CompositeDisposable();

            _sceneContextsService = _diContainer.Resolve<ISceneContextsService>();
            _sceneContextsService.OnSceneContextInstalledR3.Subscribe(OnSceneContextInstalled).AddTo(_compositeDisposable);
        }

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            return base.OnOpenAsync(cancellationToken);
        }

        private void Update()
        {
            if (_isRunning)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform) _canvasTransform.transform,
                    Input.mousePosition,
                    null,
                    out position
                );

                _imageTransform.localPosition = position;
            }

            if (_inputCoreSystem != null && _inputCoreSystem.IsSimulated.Value == true)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform) _canvasTransform.transform,
                    _inputCoreSystem.InputPosition,
                    null,
                    out position
                );

                _imageTransform.localPosition = position;
            }
        }

        private void OnSceneContextInstalled((string, SceneContext) pair)
        {
            if (pair.Item1 == "Core")
            {
                if (_inputCoreSystem != null)
                {
                    _compositeDisposable.Dispose();
                    _compositeDisposable = new CompositeDisposable();

                    _inputCoreSystem.OnScreenInputStart -= InputCoreSystemOnOnScreenInputStart;
                    _inputCoreSystem.OnScreenInputEnd -= InputCoreSystemOnOnScreenInputEnd;
                }

                _inputCoreSystem = pair.Item2.Container.Resolve<IInputCoreSystem>();
                _inputCoreSystem.IsSimulated.Subscribe(OnSimulationStateChanged).AddTo(_compositeDisposable);

                _levelExecutionCoreSystem = pair.Item2.Container.Resolve<ILevelExecutionCoreSystem>();
                _levelExecutionCoreSystem.OnLevelCompleted += LevelExecutionCoreSystemOnOnLevelCompleted;

                _inputCoreSystem.OnScreenInputStart += InputCoreSystemOnOnScreenInputStart;
                _inputCoreSystem.OnScreenInputEnd += InputCoreSystemOnOnScreenInputEnd;
                OnSimulationStateChanged(_inputCoreSystem.IsSimulated.Value);

                _isSimulating = true;
            }
        }

        private void LevelExecutionCoreSystemOnOnLevelCompleted(LevelEndReason obj)
        {
            _inputCoreSystem.IsSimulated.Value = false;
        }

        private void InputCoreSystemOnOnScreenInputStart(Vector2 obj)
        {
            _imagePressed.SetActive(true);
            _imageDefault.SetActive(false);
        }

        private void InputCoreSystemOnOnScreenInputEnd(Vector2 obj)
        {
            _imagePressed.SetActive(false);
            _imageDefault.SetActive(true);
        }

        private void OnSimulationStateChanged(bool isSimulated)
        {
            _isSimulating = true;
        }

        private void OnDestroy()
        {
            _compositeDisposable.Dispose();

            _inputCoreSystem.OnScreenInputStart -= InputCoreSystemOnOnScreenInputStart;
            _inputCoreSystem.OnScreenInputEnd -= InputCoreSystemOnOnScreenInputEnd;
        }
    }
}