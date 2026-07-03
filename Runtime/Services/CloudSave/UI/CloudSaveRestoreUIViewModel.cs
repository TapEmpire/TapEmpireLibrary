#if TEL_CLOUD_SAVE
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
            try
            {
                _cloudSaveService.DeclineRestore(_cloudDataTimestampMs);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }
            finally
            {
                CompleteAndClose(false);
            }
        }

        private async UniTaskVoid AcceptAsync()
        {
            // OnResult resolves the tcs that blocks the whole startup init — it must fire no matter what throws here
            var accepted = false;
            try
            {
                var enableResult = await _cloudSaveService.EnableAsync(CancellationToken.None);
                if (enableResult.Success)
                {
                    var restoreResult = await _cloudSaveService.RestoreAsync(CloudSnapshot, CancellationToken.None);
                    accepted = restoreResult.Success;
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }
            finally
            {
                CompleteAndClose(accepted);
            }
        }

        private void CompleteAndClose(bool accepted)
        {
            try
            {
                OnResult?.Invoke(accepted);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }

            OnResult = null;
            UIService.CloseViewAsync(this, CancellationToken.None).Forget();
        }
    }
}
#endif
