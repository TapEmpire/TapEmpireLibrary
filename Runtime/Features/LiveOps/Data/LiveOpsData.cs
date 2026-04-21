using System;
using System.Collections.Generic;
using TapEmpire.LiveOps.UI;
using UnityEngine;
using WordGame.Feature.Tutorial;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public abstract class LiveOpsData
    {
        [SerializeReference] public List<ICondition> Conditions = new();
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
        public string Id;
        public State State = State.NotStarted;
        public int Inner = 0; // Stage
        public int Value = 0;
        public int Addend = 0;
        public DateTime StartedAt;
    }
}