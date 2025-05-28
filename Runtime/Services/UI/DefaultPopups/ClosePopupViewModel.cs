using Cysharp.Threading.Tasks;
using Zenject;

namespace TapEmpire.UI
{
    public class ClosePopupViewModel : IUIViewModel, IInjectable
    {
        private IUIService _uiService;

        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
        }

        public void OnClosePressed()
        {
            _uiService.CloseViewAsync(this, default).Forget();
        }
    }
}