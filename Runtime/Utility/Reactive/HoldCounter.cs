using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Utility
{
    public sealed class HoldCounter
    {
        public ReadOnlyReactiveProperty<bool> IsHeld => _isHeld;
        public int Count => _count;

        private readonly ReactiveProperty<bool> _isHeld = new();

        private int _count;

        public IDisposable Hold()
        {
            Add(1);
            return Disposable.Create(() => Add(-1));
        }

        public void Hold(bool shouldHold)
        {
            Add(shouldHold ? 1 : -1);
        }

        public void Reset()
        {
            _count = 0;
            _isHeld.Value = false;
        }

        public async UniTask WaitReleased(CancellationToken cancellationToken = default)
        {
            await _isHeld.WaitWhen(isHeld => !isHeld, cancellationToken);
        }

        private void Add(int delta)
        {
            _count += delta;
            _isHeld.Value = _count > 0;
        }
    }
}
