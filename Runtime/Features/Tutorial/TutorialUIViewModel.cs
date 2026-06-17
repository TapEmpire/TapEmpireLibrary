using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.UI;
using Zenject;

namespace TapEmpire.Feature.Tutorial
{
    public class TutorialUIViewModel : IUIViewModel, IInjectable
    {
        public Subject<Unit> OnStepDone = new();

        private IUIService _uiService;
        private IProgressService _progressService;
        private IAdsService _adsService;

        private bool _areBannersShown = false;
        
        private readonly string _tutorialPostfix;
        public bool IsSkippable { get; }

        public TutorialUIViewModel(string tutorialPostfix = "", bool isSkippable = false)
        {
            _tutorialPostfix = tutorialPostfix;
            IsSkippable = isSkippable;
        }

        [Inject]
        private void Construct(IUIService uiService, IProgressService progressService, IAdsService adsService)
        {
            _uiService = uiService;
            _progressService = progressService;
            _adsService = adsService;

            _areBannersShown = adsService.ShowBanner(false);
        }

        public void OnNextPressed()
        {
            _progressService.SetTutorialProgress(_tutorialPostfix, 1);
            OnStepDone.OnNext(Unit.Default);
            _adsService.ShowBanner(_areBannersShown);
            _uiService.CloseViewAsync(this, CancellationToken.None).Forget();
        }
    }
}