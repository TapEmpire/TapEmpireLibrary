using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class CustomCursorService : Initializable, ICustomCursorService
    {
        [SerializeField] private CustomCursorUIView _uiView;
        [SerializeField] private string[] _contexts = new []{"Menu", "Core"};

        private IUIService _uiService;
        private DiContainer _diContainer;
        private ISceneContextsService _sceneContextsService;
        
        private bool _isInitialized = false;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IUIService uiService, DiContainer diContainer, ISceneContextsService sceneContextsService)
        {
            _uiService = uiService;
            _diContainer = diContainer;
            _sceneContextsService = sceneContextsService;
            _isInitialized = false;

            _sceneContextsService.OnSceneContextInstalledR3.Subscribe(OnContextInitialized).AddTo(_disposables);
            // _uiView.SetInfo(_diContainer);
        }
        
        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _uiService.TryCloseViewAsync<CustomCursorUIViewModel>();
            
            _disposables.Dispose();
            
            base.OnRelease();
        }

        private void OnContextInitialized((string, SceneContext) pair)
        {
            if (_isInitialized) return;
            
            if (HasContext(pair.Item1))
            {
                _isInitialized = true;
                _uiService.OpenViewAsync(_uiView, new CustomCursorUIViewModel(), CancellationToken.None);
            }
        }

        private bool HasContext(string name)
        {
            return _contexts.Any(t => name == t);
        }
    }
}