using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public class BannerAdMediator : IBanner
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public ReactiveProperty<Vector2> Layout { get; } = new(Vector2.zero);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly IReadOnlyList<IBanner> _providers;
        private readonly CompositeDisposable _disposables = new();

        private bool _shouldShow;

        public BannerAdMediator(IReadOnlyList<IBanner> providers)
        {
            _providers = providers;

            foreach (var provider in _providers)
            {
                provider.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_disposables);
                provider.IsLoaded
                    .Subscribe(_ => IsLoaded.Value = _providers.Any(provider => provider.IsLoaded.Value))
                    .AddTo(_disposables);
                provider.Layout
                    .Subscribe(layout => Layout.Value = layout)
                    .AddTo(_disposables);
            }

            _providers[0].IsLoaded.Subscribe(Refresh).AddTo(_disposables);
        }

        public void Show()
        {
            _shouldShow = true;
            var provider = _providers.FirstOrDefault(provider => provider.IsLoaded.Value);
            provider?.Show();
        }

        public void Hide()
        {
            _shouldShow = false;
            foreach (var provider in _providers) provider.Hide();
        }

        private void Refresh(bool isLoaded)
        {
            if (isLoaded)
            {
                foreach (var provider in _providers.Skip(1)) provider.Hide();
                Apply(_providers[0]);
            }
            else
            {
                _providers[0].Hide();
                Apply(_providers.Skip(1).FirstOrDefault(provider => provider.IsLoaded.Value));
            }
        }

        private void Apply(IBanner provider)
        {
            if (provider == null) return;
            if (_shouldShow) provider.Show();
            else provider.Hide();
        }

        public void Dispose() => _disposables.Dispose();
    }
}
