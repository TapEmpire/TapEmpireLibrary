using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Services.Analytics;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsAnalyticsModule : IDisposable
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILiveOpsService _liveOpsService;
        private readonly IProgressService _progressService;

        private CompositeDisposable _disposables = new();

        public LiveOpsAnalyticsModule(DiContainer diContainer)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _liveOpsService = diContainer.Resolve<ILiveOpsService>();
            _progressService = diContainer.Resolve<IProgressService>();

            // _liveOpsService.OnLiveOpsShown.Subscribe(OnLiveOpsShown).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        // private void OnLiveOpsShown((LiveOpsType LiveOpsType, bool Autoshown, string Placement) data)
        // {
        //     var level = _progressService.GetVisualProgress();

        //     _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>()
        //     {
        //         { "LiveOpss", new JObject(new JProperty(data.LiveOpsType.ToString(),
        //             new JObject(new JProperty(data.Placement, $"Level_{level}")))) }
        //     });
        // }
    }
}