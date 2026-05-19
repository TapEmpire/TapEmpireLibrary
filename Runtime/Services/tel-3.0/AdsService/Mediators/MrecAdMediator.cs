using System;
using System.Collections.Generic;
using R3;

namespace TapEmpire.Experimental
{
    public class MrecAdMediator : IMrec, IDisposable
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly IReadOnlyList<IMrec> _providers;
        private readonly CompositeDisposable _disposables = new();

        private IMrec _activeProvider;

        public MrecAdMediator(IReadOnlyList<IMrec> providers)
        {
            _providers = providers;

            foreach (var provider in _providers)
            {
                provider.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_disposables);
                provider.IsLoaded
                    .Where(_ => provider == _activeProvider)
                    .Subscribe(loaded => IsLoaded.Value = loaded)
                    .AddTo(_disposables);
            }
        }

        public void Show()
        {
            ShowOnPreferred(provider => provider.Show());
        }

        public void Show(int x, int y)
        {
            ShowOnPreferred(provider => provider.Show(x, y));
        }

        public void Hide()
        {
            foreach (var provider in _providers) provider.Hide();
            _activeProvider = null;
            IsLoaded.Value = false;
        }

        private void ShowOnPreferred(Action<IMrec> show)
        {
            IMrec preferred = null;
            foreach (var provider in _providers)
            {
                if (provider.IsLoaded.Value) { preferred = provider; break; }
            }
            preferred ??= _providers[0];

            foreach (var provider in _providers)
            {
                if (provider != preferred) provider.Hide();
            }
            show(preferred);
            _activeProvider = preferred;
            IsLoaded.Value = preferred.IsLoaded.Value;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
