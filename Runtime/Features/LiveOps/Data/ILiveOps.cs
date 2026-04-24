using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOps : IDisposable
    {
        StateData StateData { get; }
        string Name { get; }
        Observable<StateData> OnDataChanged { get; }
        Observable<ILiveOps> OnStarted { get; }
        Observable<ILiveOps> OnStage { get; }
        Observable<ILiveOps> OnFinished { get; }

        IDisposable CreateIcon(Transform parent);
        UniTask OpenView();
        UniTask OpenTutorial();
        TimeSpan GetRemainingTime();
    }

    public enum State
    {
        NotStarted,
        Starting,
        Active,
        Finished,
    }

    public enum UpdateAction
    {
        None,
        Start,
        Update,
        Finish,
    }

}