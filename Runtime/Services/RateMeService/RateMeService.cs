using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;
using TapEmpire.UI;
using R3;

namespace TapEmpire.Services
{
    [Serializable]
    public class RateMeService : Initializable, IRateMeService
    {
        [SerializeField] private RateMeSettings _rateMeSettings;
        [SerializeField] private RateMeUIView _rateMeUiView;

        public bool HasRated => _progressService.GetRateMe();

        private IProgressService _progressService = null;
        private IUIService _uiService = null;

        private CancellationTokenSource _cancellationTokenSource;
        private CompositeDisposable _disposables;
        private RateMeUiViewModel _rateMeUiViewModel;
        private IReviewManager _reviewManager;

        private UniTaskCompletionSource<bool> _completionSource = null;

        [Inject]
        private void Construct(IProgressService progressService, IUIService uiService)
        {
            _progressService = progressService;
            _uiService = uiService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new CompositeDisposable();
            _cancellationTokenSource = new CancellationTokenSource();
            _rateMeUiViewModel = new RateMeUiViewModel();
            _reviewManager = CreateReviewManager();
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _uiService.TryCloseViewAsync<RateMeUiViewModel>(_cancellationTokenSource.Token).Forget();

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }

            _disposables?.Dispose();
            _rateMeUiViewModel = null;
            _reviewManager = null;
        }

        public async UniTask Rate()
        {
            _completionSource = new UniTaskCompletionSource<bool>();
            _uiService.OpenViewAsync(_rateMeUiView, _rateMeUiViewModel, _cancellationTokenSource.Token).Forget();
            _rateMeUiViewModel.AcceptCommand.Subscribe(_ => Accept()).AddTo(_disposables);
            _rateMeUiViewModel.RejectCommand.Subscribe(_ => Reject()).AddTo(_disposables);
            await _completionSource.Task;
        }

        public async UniTask RateOnLevel(int level)
        {
            if (IsNeedRated(level))
            {
                await Rate();
            }
        }

        public bool IsNeedRated(int level)
        {
            if (!_rateMeSettings.Enable || HasRated)
            {
                return false;
            }
            
            var shouldShowRateMe = _rateMeSettings.Levels.Exists(rateLevel => rateLevel == level);
            return shouldShowRateMe;
        }

        private void Accept()
        {
            _progressService.SetRateMe(true);
            _reviewManager?.RateMeAsync(_cancellationTokenSource.Token).Forget();
            _uiService.TryCloseViewAsync<RateMeUiViewModel>(_cancellationTokenSource.Token).Forget();
            _completionSource.TrySetResult(true);
        }

        private void Reject()
        {
            _uiService.TryCloseViewAsync<RateMeUiViewModel>(_cancellationTokenSource.Token).Forget();
            _completionSource.TrySetResult(false);
        }

        private IReviewManager CreateReviewManager()
        {
#if UNITY_ANDROID
            return new AndroidReviewManager();
#elif UNITY_IOS
            return new IosReviewManager();
#else
            return null;
#endif
        }
    }
}