using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.UI;
using Zenject;

namespace TapEmpire.Services
{
    public class CloudSaveRestoreUIViewModel : IUIViewModel, IInjectable
    {
        public DateTime CloudDataDate { get; }
        
        public IProgressService ProgressService => _progressService;
        public IUIService UIService => _uiService;
        public DiContainer Container => _container;

        public Action<bool> OnResult;
        
        public readonly ProgressSnapshot CloudSnapshot;
        
        private IUIService _uiService;
        private ICloudSaveService _cloudSaveService;
        private IProgressService _progressService;
        
        private readonly long _cloudDataTimestampMs;
        private readonly DiContainer _container;

        public CloudSaveRestoreUIViewModel(DiContainer container, long cloudDataTimestampMs, ProgressSnapshot cloudSnapshot)
        {
            _cloudDataTimestampMs = cloudDataTimestampMs;
            CloudSnapshot = cloudSnapshot;
            CloudDataDate = DateTimeOffset.FromUnixTimeMilliseconds(cloudDataTimestampMs).DateTime;

            _uiService = container.Resolve<IUIService>();
            _progressService = container.Resolve<IProgressService>();
            _cloudSaveService = container.Resolve<ICloudSaveService>();
            _container = container;
        }
        
        public CloudSaveRestoreUIViewModel(long cloudDataTimestampMs, ProgressSnapshot cloudSnapshot)
        {
            _cloudDataTimestampMs = cloudDataTimestampMs;
            CloudSnapshot = cloudSnapshot;
        }

        public void OnAcceptPressed()
        {
            AcceptAsync().Forget();
        }

        public void OnDeclinePressed()
        {
            _cloudSaveService.DeclineRestore(_cloudDataTimestampMs);
            OnResult?.Invoke(false);
            OnResult = null;
            UIService.CloseViewAsync(this, CancellationToken.None).Forget();
        }
        
        private async UniTaskVoid AcceptAsync()
        {
            var enableResult = await _cloudSaveService.EnableAsync(CancellationToken.None);
            if (!enableResult.Success)
            {
                OnResult?.Invoke(false);
                OnResult = null;
                UIService.CloseViewAsync(this, CancellationToken.None).Forget();
                return;
            }

            var restoreResult = await _cloudSaveService.RestoreAsync(CancellationToken.None);
            
            OnResult?.Invoke(restoreResult.Success);
            OnResult = null;
            UIService.CloseViewAsync(this, CancellationToken.None).Forget();
        }
    }
}
