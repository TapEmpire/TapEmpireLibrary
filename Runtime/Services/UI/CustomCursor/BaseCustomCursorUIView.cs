using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.UI
{
    public class BaseCustomCursorUIView : UIView<CustomCursorUIViewModel>, IInjectable
    {
        [SerializeField] protected RectTransform _imageTransform;
        [SerializeField] protected GameObject _imageDefault;
        [SerializeField] protected GameObject _imagePressed;

        protected DiContainer _diContainer;
        protected ISceneContextsService _sceneContextsService;

        protected RectTransform _canvasTransform;
        protected bool _isRunning;
        protected bool _isSimulating;

        protected CompositeDisposable _compositeDisposable;

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            var canvas = GetComponentInParent<Canvas>();
            _canvasTransform = (RectTransform) canvas.transform;

            _isRunning = true;

            return base.OpenAsync(cancellationToken);
        }

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _isRunning = false;
            
            return base.OnOpenAsync(cancellationToken);
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
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform) _canvasTransform.transform,
                    Input.mousePosition,
                    null,
                    out position
                );

                _imageTransform.localPosition = position;
            }
        }

        protected virtual void OnSceneContextInstalled((string, SceneContext) pair)
        {
        }

        protected virtual void OnDestroy()
        {
            _compositeDisposable.Dispose();
        }
    }
}