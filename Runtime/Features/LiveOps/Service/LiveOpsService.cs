using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.LiveOps.UI;
using UnityEngine;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsService : Initializable, ILiveOpsService
    {
        [field: SerializeField] public LiveOpsSettings Settings { get; private set; }

        private DiContainer _diContainer;

        public IReadOnlyList<ILiveOps> LiveOps => _liveOps;

        private List<ILiveOps> _liveOps = new();
        private CompositeDisposable _disposables = new();
        private Transform _resourceEmitter = null;

        [Inject]
        private void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new();
            _liveOps = new();

            Settings.LiveOps.ForEach(InitializeAndRegisterLiveOps);

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _liveOps.Clear();
            _disposables.Dispose();
            base.OnRelease();
        }

        public T GetLiveOps<T>() where T : ILiveOps
        {
            return _liveOps.OfType<T>().FirstOrDefault();
        }

        public async UniTask UpdateLiveOps(LiveOpsIconLayout layout = null, bool debug = false)
        {
            foreach (var liveOps in _liveOps)
                liveOps.UpdatePrepare(debug);

            var active = _liveOps.Where(liveOps => liveOps.Runtime.State != State.NotStarted).ToList();

            // Finished events with announce icon: timer still running (can't restart yet), previously started.
            var lockedVisible = _liveOps
                .Where(liveOps => liveOps.Runtime.State == State.NotStarted &&
                                  liveOps.Data.HasAnnounceIcon &&
                                  liveOps.Runtime.StartedAt != default &&
                                  liveOps.GetRemainingTime() > TimeSpan.Zero)
                .ToList();

            var inactive = _liveOps
                .Where(liveOps => liveOps.Runtime.State == State.NotStarted && !lockedVisible.Contains(liveOps))
                .ToList();

            if (layout != null)
            {
                active.ForEach(liveOps => liveOps.CreateIcon()?.AddTo(layout));
                lockedVisible.ForEach(liveOps => liveOps.CreateAnnounceIcon()?.AddTo(layout));
            }

            await UniTask.WaitForSeconds(Settings.UpdateDelaySeconds, cancellationToken: default);

            if (active.Count > 0)
            {
                await active[0].UpdateVisual(_resourceEmitter, debug);
                var tasks = new List<UniTask>(active.Count - 1);
                for (var i = 1; i < active.Count; i++)
                {
                    await UniTask.WaitForSeconds(Settings.UpdateVisualIntervalSeconds, cancellationToken: default);
                    tasks.Add(active[i].UpdateVisual(_resourceEmitter, debug));
                }
                if (tasks.Count > 0)
                    await UniTask.WhenAll(tasks);
            }

            foreach (var liveOps in active)
                await liveOps.UpdatePopups();

            foreach (var liveOps in inactive)
            {
                await liveOps.UpdatePopups();

                if (layout == null)
                    continue;

                if (liveOps.Runtime.State != State.NotStarted)
                    liveOps.CreateIcon()?.AddTo(layout);
            }
        }

        private void InitializeAndRegisterLiveOps(LiveOpsData data)
        {
            var liveOps = data.Create(_diContainer);
            liveOps.AddTo(_disposables);
            _liveOps.Add(liveOps);
        }
    }
}
