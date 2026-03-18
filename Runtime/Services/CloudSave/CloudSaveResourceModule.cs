using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Zenject;

namespace TapEmpire.Services
{
    public class CloudSaveResourceModule<T>
    {
        private readonly ICloudSaveService _cloudSaveService;
        private readonly CompositeDisposable _disposables = new();

        public CloudSaveResourceModule(DiContainer diContainer, IResourcesService<T> resourcesService)
        {
            if (!diContainer.HasBinding<ICloudSaveService>())
                return;

            _cloudSaveService = diContainer.Resolve<ICloudSaveService>();

            resourcesService.OnResourceAdded.Subscribe(_ => Save()).AddTo(_disposables);
            resourcesService.OnResourceUsed.Subscribe(_ => Save()).AddTo(_disposables);
            resourcesService.OnVirtualAdded.Subscribe(_ => Save()).AddTo(_disposables);
            resourcesService.OnResourceAddedDetailed.Subscribe(_ => Save()).AddTo(_disposables);
            resourcesService.OnResourceUsedDetailed.Subscribe(_ => Save()).AddTo(_disposables);
        }

        public void OnRelease()
        {
            _disposables.Dispose();
        }

        private void Save()
        {
            if (_cloudSaveService is not {IsEnabled: true})
                return;

            _cloudSaveService.SaveAsync(CancellationToken.None).Forget();
        }
    }
}
