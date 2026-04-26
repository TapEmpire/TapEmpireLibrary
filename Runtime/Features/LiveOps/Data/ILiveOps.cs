using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOps : IDisposable
    {
        LiveOpsRuntime Runtime { get; }
        string Name { get; }
        Observable<LiveOpsRuntime> OnDataChanged { get; }
        Observable<ILiveOps> OnStarted { get; }
        Observable<ILiveOps> OnStage { get; }
        Observable<ILiveOps> OnFinished { get; }
        Observable<ILiveOps> OnExpired { get; }

        IDisposable CreateIcon(Transform parent);
        UniTask OpenView();
        UniTask OpenTutorial(bool isSkippable = true);
        TimeSpan GetRemainingTime();
        void Save();
        void UpdatePrepare(bool debug = false);
        UniTask UpdateVisual(Transform from, bool debug = false);
        UniTask UpdatePopups();
    }

    public enum State
    {
        NotStarted,
        Starting,
        Active,
        Finished,
    }

}