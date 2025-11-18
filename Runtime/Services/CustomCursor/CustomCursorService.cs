using System;
using System.Linq;
using System.Threading;
using R3;
using TapEmpire.UI;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class CustomCursorService : Initializable, ICustomCursorService
    {
        [SerializeField] private BaseCustomCursorUIView _uiView;
        [SerializeField] private string[] _contexts = new []{"Menu", "Core"};

        private IUIService _uiService;
        private ISceneContextsService _sceneContextsService;
        
        private bool _isInitialized = false;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IUIService uiService, ISceneContextsService sceneContextsService)
        {
            _uiService = uiService;
            _sceneContextsService = sceneContextsService;
            _isInitialized = false;

            _sceneContextsService.OnSceneContextInstalledR3.Subscribe(OnContextInitialized).AddTo(_disposables);
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