
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility
{
    public class CancellableTask : IDisposable
    {
        private CancellationTokenSource _cts;
        private UniTask _task;

        public CancellableTask(Func<CancellationToken, UniTask> taskFactory)
        {
            _cts = new CancellationTokenSource();
            _task = taskFactory(_cts.Token);
        }

        public void Cancel()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        public void Dispose()
        {
            Cancel();
            _cts.Dispose();
        }

        public UniTask AsUniTask()
        {
            return _task;
        }
    }
}