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

        void Initialize(DiContainer diContainer, LiveOpsData settings);
        IDisposable CreateIcon(Transform parent);
        void OpenView();
        void OpenTutorial();

        UpdateAction CheckUpdateAction(); // return what action we should for updateVisual

        void UpdateData(string placement); // Forward call. Try not to use.
        UniTask UpdateVisual(Transform from, bool debug = false); // Flying step
        UniTask UpdateState(); // Popup state

        void Save();
    }

    public enum State
    {
        NotStarted,
        Active,
        Finished,
        Closed,
    }

    public enum UpdateAction
    {
        None,
        Start,
        Update,
        Finish,
    }

    // placement? или подписки? или подписка + messagebus?
    // point of visual update
}