using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.UI;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsTutorialViewModel : IUIViewModel, IInjectable
    {
        public Subject<Unit> OnStepDone = new();

        private IUIService _uiService;
        private IProgressService _progressService;
        private IAdsService _adsService;

        private bool _areBannersShown = false;

        [Inject]
        private void Construct(IUIService uiService, IProgressService progressService, IAdsService adsService)
        {
            _uiService = uiService;
            _progressService = progressService;
            _adsService = adsService;

            _areBannersShown = adsService.ShowBanners(false);
        }

        public void OnNextPressed()
        {
            _progressService.SetTutorialProgress(1);
            OnStepDone.OnNext(Unit.Default);
            _adsService.ShowBanners(_areBannersShown);
            _uiService.CloseViewAsync(this, CancellationToken.None).Forget();
        }
    }
}