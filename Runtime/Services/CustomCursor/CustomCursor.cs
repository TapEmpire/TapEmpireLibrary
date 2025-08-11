using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using UnityEngine;
using WordGame.CoreSystems;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class CustomCursor : Initializable, ICustomCursorService
    {
        [SerializeField] private CustomCursorUIView _uiView;

        private IUIService _uiService;
        private DiContainer _diContainer;
        private ISceneContextsService _sceneContextsService;
        
        [Inject]
        private void Construct(IUIService uiService, DiContainer diContainer, ISceneContextsService sceneContextsService)
        {
            _uiService = uiService;
            _diContainer = diContainer;
            _sceneContextsService = sceneContextsService;
            _isInitialized = false;

            _sceneContextsService.OnSceneContextInstalledR3.Subscribe(OnContextInitialized);
            // _uiView.SetInfo(_diContainer);
        }
        
        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _uiService.TryCloseViewAsync<CustomCursorUIViewModel>();
            base.OnRelease();
        }

        private bool _isInitialized = false;
        private void OnContextInitialized((string, SceneContext) pair)
        {
            if (_isInitialized) return;
            
            if (pair.Item1 == "Core" || pair.Item1 == "Menu")
            {
                _isInitialized = true;
                _uiService.OpenViewAsync(_uiView, new CustomCursorUIViewModel(), CancellationToken.None);
            }
        }
    }
}