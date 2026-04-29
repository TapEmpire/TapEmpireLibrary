using System;
using TapEmpire.LiveOps.UI;
using TapEmpire.Feature.Tutorial;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public abstract class LiveOpsData
    {
        public LiveOpsIcon IconPrefab;
        public LiveOpsView LiveOpsPrefab;
        public TutorialUIView TutorialPrefab;
        public LiveOpsDebugComponent DebugComponent;
        public abstract string Name { get; }
        public abstract ILiveOps Create(DiContainer container);
        public abstract DateTime GetEndTime(LiveOpsRuntime runtime);
    }

    public class LiveOpsRuntime
    {
        public string Id = null;
        public State State = State.NotStarted;
        public int Inner = 0; // Stage
        public int Value = 0;
        public int Addend = 0;
        public DateTime StartedAt;

        public LiveOpsRuntime() { }

        public LiveOpsRuntime(string id)
        {
            Id = id;
            State = State.Active;
            StartedAt = DateTime.UtcNow;
        }
    }
}