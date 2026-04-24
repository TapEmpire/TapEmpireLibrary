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
        public abstract ILiveOps Create();
        
        //todo: possible future implementation 
        // public abstract IReadOnlyList<ILiveOpsParams> AllParams { get; }
        // public abstract ILiveOpsParams ActiveParams { get; }
        // public abstract T GetLiveOpsParams<T>() where T : ILiveOpsParams;
        // public abstract void SetActiveParamsById(string id);
        // public abstract void SetActiveParamsByIndex(int index);
        // public abstract ILiveOpsParams DeserializeParams(JToken token);
        // public abstract void AddOrReplaceParams(ILiveOpsParams p);
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