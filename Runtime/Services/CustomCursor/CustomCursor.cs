using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.UI;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class CustomCursor : Initializable, ICustomCursorService
    {
        [SerializeField] private CustomCursorUIView _uiView;

        private IUIService _uiService;
        
        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
        }
        
        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _uiService.OpenViewAsync(_uiView, new CustomCursorUIViewModel(), CancellationToken.None);
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _uiService.TryCloseViewAsync<CustomCursorUIViewModel>();
            base.OnRelease();
        }
    }
}