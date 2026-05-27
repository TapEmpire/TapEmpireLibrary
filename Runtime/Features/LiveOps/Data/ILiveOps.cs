using System;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.LiveOps.UI;
using UnityEngine;
namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOps : IDisposable
    {
        LiveOpsData Data { get; }
        LiveOpsRuntime Runtime { get; }
        string Name { get; }
        Observable<LiveOpsRuntime> OnDataChanged { get; }
        Observable<ILiveOps> OnStarted { get; }
        Observable<ILiveOps> OnStage { get; }
        Observable<ILiveOps> OnFinished { get; }
        Observable<ILiveOps> OnExpired { get; }

        LiveOpsIcon CreateIcon();
        LiveOpsIcon CreateAnnounceIcon();
        UniTask OpenView();
        UniTask OpenTutorial(bool isSkippable = true);
        TimeSpan GetRemainingTime();
        void SetEndTime(DateTime endTime);
        void Save();
        void UpdatePrepare(bool debug = false);
        UniTask UpdateVisual(Transform from, bool debug = false);
        UniTask UpdatePopups();
    }

    public enum State
    {
        NotStarted,
        Active,
        Finished,
    }

}