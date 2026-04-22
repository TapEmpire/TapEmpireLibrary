using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;

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

        void Initialize(DiContainer diContainer, LiveOpsData settings);
        IDisposable CreateIcon(Transform parent);
        void OpenView();
        void OpenTutorial();

        UpdateAction UpdatePrepare(bool debug = false);

        UniTask UpdateVisual(Transform from, bool debug = false);
        UniTask UpdatePopups();

        void Save();
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