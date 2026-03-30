using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Services.Shop;
using TapEmpire.UI;
using Zenject;
using TapEmpire.Services.LiveOps;

namespace TapEmpire.LiveOps.UI
{
    public class LiveOpsViewModel : IUIViewModel, IInjectable
    {
        public ILiveOps LiveOps { get; private set; }
        public IUIService UiService;

        public T As<T>() where T : ILiveOps => (T)LiveOps;

        [Inject]
        private void Construct(IUIService uiService)
        {
            UiService = uiService;
        }

        public LiveOpsViewModel(ILiveOps liveOps)
        {
            LiveOps = liveOps;
        }

        public void Close()
        {
            UiService.CloseViewAsync(this, default).Forget();
        }

        public void OpenInfo() => LiveOps.OpenTutorial();
    }
}