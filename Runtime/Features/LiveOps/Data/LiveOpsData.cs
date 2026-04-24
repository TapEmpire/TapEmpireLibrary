using System;
using TapEmpire.LiveOps.UI;
using TapEmpire.Feature.Tutorial;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public abstract class LiveOpsData
    {
        public int MinStartLevelIndex;
        public LiveOpsIcon IconPrefab;
        public LiveOpsView LiveOpsPrefab;
        public TutorialUIView TutorialPrefab;
        public abstract string Name { get; }
        public abstract LiveOpsBase Create();
    }

    public class StateData
    {
        public string Id = null;
        public State State = State.NotStarted;
        public int Inner = 0; // Stage
        public int Value = 0;
        public int Addend = 0;
        public DateTime StartedAt;

        public StateData() { }

        public StateData(string id)
        {
            Id = id;
            State = State.Active;
            StartedAt = DateTime.UtcNow;
        }
    }
}