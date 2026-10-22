using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.CoreSystems;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.UI
{
    public class CursorUIView : UIView<CursorUIViewModel>, IInjectable
    {
        [SerializeField] protected RectTransform _imageTransform;
        [SerializeField] protected GameObject _imageDefault;
        [SerializeField] protected GameObject _imagePressed;

        protected DiContainer _diContainer;
        protected ISceneContextsService _sceneContextsService;
        protected IInputCoreSystem _inputCoreSystem;

        protected RectTransform _canvasTransform;
        protected bool _isRunning;

        protected CompositeDisposable _compositeDisposable;
        private CompositeDisposable _inputDisposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            var canvas = GetComponentInParent<Canvas>();
            _canvasTransform = (RectTransform) canvas.transform;

            _isRunning = true;

            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _isRunning = false;

            return base.OnCloseAsync(cancellationToken);
        }

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;

            _compositeDisposable = new CompositeDisposable();

            _sceneContextsService = _diContainer.Resolve<ISceneContextsService>();
            _sceneContextsService.OnSceneContextInstalledR3.Subscribe(OnSceneContextInstalled).AddTo(_compositeDisposable);
        }

        protected virtual void Update()
        {
            if (_isRunning)
            {
                var screenPosition = _inputCoreSystem != null && _inputCoreSystem.IsSimulated
                    ? _inputCoreSystem.InputPosition
                    : (Vector2) Input.mousePosition;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, screenPosition, null, out var position);

                _imageTransform.localPosition = position;
            }
        }

        protected virtual void OnSceneContextInstalled((string, SceneContext) pair)
        {
            var inputCoreSystem = pair.Item2.Container.TryResolve<IInputCoreSystem>();
            if (inputCoreSystem == null)
            {
                return;
            }

            _inputDisposables.Dispose();
            _inputDisposables = new CompositeDisposable();

            _inputCoreSystem = inputCoreSystem;
            _inputCoreSystem.OnInputStart.Subscribe(_ => SetPressed(true)).AddTo(_inputDisposables);
            _inputCoreSystem.OnInputEnd.Subscribe(_ => SetPressed(false)).AddTo(_inputDisposables);
        }

        private void SetPressed(bool isPressed)
        {
            _imagePressed.SetActive(isPressed);
            _imageDefault.SetActive(!isPressed);
        }

        protected virtual void OnDestroy()
        {
            _compositeDisposable.Dispose();
            _inputDisposables.Dispose();
        }
    }
}
