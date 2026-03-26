using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.LiveOps.UI
{
    public class LiveOpsView : UIView<LiveOpsViewModel>, IInjectable
    {
        protected readonly CompositeDisposable _disposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();

            return base.OnCloseAsync(cancellationToken);
        }
    }
}
