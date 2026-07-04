using System.Collections.Generic;
using System.Linq;
using R3;

namespace TapEmpire.Services
{
    public class RewardedAdMediator : IRewarded
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public ReactiveProperty<bool> IsShowing { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();

        private readonly List<IRewarded> _providers = new();
        private readonly CompositeDisposable _disposables = new();

        public void AddProvider(IRewarded provider)
        {
            _providers.Insert(0, provider);

            provider.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_disposables);
            provider.OnReward.Subscribe(OnReward.OnNext).AddTo(_disposables);
            provider.IsLoaded
                .Subscribe(_ => IsLoaded.Value = _providers.Any(entry => entry.IsLoaded.Value))
                .AddTo(_disposables);
            provider.IsShowing
                .Subscribe(_ => IsShowing.Value = _providers.Any(entry => entry.IsShowing.Value))
                .AddTo(_disposables);
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
