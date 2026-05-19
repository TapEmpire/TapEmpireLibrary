using System.Collections.Generic;
using System.Linq;
using R3;

namespace TapEmpire.Experimental
{
    public class RewardedAdMediator : IRewarded
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();

        private readonly IReadOnlyList<IRewarded> _providers;
        private readonly CompositeDisposable _disposables = new();

        public RewardedAdMediator(IReadOnlyList<IRewarded> providers)
        {
            _providers = providers;
            foreach (var provider in _providers)
            {
                provider.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_disposables);
                provider.OnReward.Subscribe(OnReward.OnNext).AddTo(_disposables);
                provider.IsLoaded
                    .Subscribe(_ => IsLoaded.Value = _providers.Any(provider => provider.IsLoaded.Value))
                    .AddTo(_disposables);
            }
        }

        public bool HasRewarded(bool doRequest = false)
        {
            return _providers.Any(provider => provider.HasRewarded(doRequest));
        }

        public void Show(string placement)
        {
            foreach (var provider in _providers)
            {
                if (provider.HasRewarded(true))
                {
                    provider.Show(placement);
                    return;
                }
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
