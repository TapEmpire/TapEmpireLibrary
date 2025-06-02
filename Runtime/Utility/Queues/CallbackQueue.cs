
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Utility
{
    public class CallbackQueue
    {
        private ReactiveProperty<bool> _isProcessing = new(false);
        public ReadOnlyReactiveProperty<bool> IsProcessing => _isProcessing;

        private Queue<Func<UniTask>> _queue = new();

        public int Count => _queue.Count;


        public CallbackQueue Enqueue(Func<UniTask> queueItem)
        {
            _queue.Enqueue(queueItem);

            if (!_isProcessing.Value)
            {
                _isProcessing.Value = true;
                Process().Forget();
            }

            return this;
        }

        public CallbackQueue Clear()
        {
            _queue.Clear();
            _isProcessing.Value = false;
            return this;
        }

        private async UniTask Process()
        {
            while (_queue.NonEmpty())
            {
                var callback = _queue.Dequeue();
                await callback();
            }

            _isProcessing.Value = false;
        }
    }
}