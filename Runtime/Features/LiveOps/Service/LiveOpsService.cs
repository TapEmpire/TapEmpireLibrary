using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsService : Initializable, ILiveOpsService
    {
        [field: SerializeField] public LiveOpsSettings Settings { get; private set; }

        private DiContainer _diContainer;

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

        public async UniTask UpdateLiveOps(List<Transform> placements = null, bool debug = false)
        {
            foreach (var liveOps in _liveOps)
                liveOps.UpdatePrepare(debug);

            var active = _liveOps.Where(liveOps => liveOps.Runtime.State != State.NotStarted);
            var inactive = _liveOps.Where(liveOps => liveOps.Runtime.State == State.NotStarted);

            if (placements != null)
                active.ForEach((liveOps, index) => liveOps.CreateIcon(placements[index]));

            await UniTask.WaitForSeconds(Settings.UpdateDelaySeconds, cancellationToken: default);
            await UniTask.WhenAll(active.Select(liveOps => liveOps.UpdateVisual(_resourceEmitter, debug)));

            foreach (var liveOps in active)
                await liveOps.UpdatePopups();

            foreach (var liveOps in inactive)
            {
                await liveOps.UpdatePopups();

                if (placements != null && liveOps.Runtime.State != State.NotStarted)
                    liveOps.CreateIcon(placements[_liveOps.Count - 1]);
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
