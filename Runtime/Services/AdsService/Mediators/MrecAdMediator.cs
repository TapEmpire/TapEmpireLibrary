using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MrecAdMediator : IMrec
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly IReadOnlyList<IMrec> _providers;
        private readonly CompositeDisposable _disposables = new();

        private bool _shouldShow;
        private Vector2Int? _customPosition;

        public MrecAdMediator(IReadOnlyList<IMrec> providers)
        {
            _providers = providers;

            foreach (var provider in _providers)
            {
                provider.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_disposables);
                provider.IsLoaded
                    .Subscribe(_ => IsLoaded.Value = _providers.Any(provider => provider.IsLoaded.Value))
                    .AddTo(_disposables);
            }

            _providers[0].IsLoaded.Subscribe(Refresh).AddTo(_disposables);
        }

        public void Show()
        {
            _shouldShow = true;
            _customPosition = null;
            _providers.FirstOrDefault(provider => provider.IsLoaded.Value)?.Show();
        }

        public void Show(int x, int y)
        {
            _shouldShow = true;
            _customPosition = new Vector2Int(x, y);
            _providers.FirstOrDefault(provider => provider.IsLoaded.Value)?.Show(x, y);
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

        private void Apply(IMrec provider)
        {
            if (_shouldShow) ShowInternal(provider);
            else provider.Hide();
        }

        private void ShowInternal(IMrec provider)
        {
            if (_customPosition.HasValue) provider.Show(_customPosition.Value.x, _customPosition.Value.y);
            else provider.Show();
        }

        public void Dispose() => _disposables.Dispose();
    }
}
