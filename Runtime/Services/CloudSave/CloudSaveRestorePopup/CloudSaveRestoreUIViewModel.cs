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
        public Action<bool> OnResult;

        private readonly long _cloudDataTimestampMs;
        private IUIService _uiService;
        private ICloudSaveService _cloudSaveService;

        public CloudSaveRestoreUIViewModel(long cloudDataTimestampMs)
        {
            _cloudDataTimestampMs = cloudDataTimestampMs;
            CloudDataDate = DateTimeOffset.FromUnixTimeMilliseconds(cloudDataTimestampMs).DateTime;
        }

        [Inject]
        private void Construct(IUIService uiService, ICloudSaveService cloudSaveService)
        {
            _uiService = uiService;
            _cloudSaveService = cloudSaveService;
        }

        public void OnAcceptPressed()
        {
            AcceptAsync().Forget();
        }

        private async UniTaskVoid AcceptAsync()
        {
            var enableResult = await _cloudSaveService.EnableAsync(CancellationToken.None);
            if (!enableResult.Success)
            {
                OnResult?.Invoke(false);
                OnResult = null;
                _uiService.CloseViewAsync(this, default).Forget();
                return;
            }

            var restoreResult = await _cloudSaveService.RestoreAsync(CancellationToken.None);
            OnResult?.Invoke(restoreResult.Success);
            OnResult = null;
            _uiService.CloseViewAsync(this, default).Forget();
        }

        public void OnDeclinePressed()
        {
            _cloudSaveService.DeclineRestore(_cloudDataTimestampMs);
            OnResult?.Invoke(false);
            OnResult = null;
            _uiService.CloseViewAsync(this, default).Forget();
        }
    }
}
