using System.Collections.Generic;
using UnityEngine;
using TEL.Utilities;

namespace TEL.GraphTool.Data
{
    using ScriptableObjects;

    public class GTEditorGraphDataSO : ScriptableObject
    {
        [field: SerializeField] public List<GTGroupData> Groups { get; set; }
        [field: SerializeField] public List<GTNodeEditorData> EditorNodes { get; set; }
        [field: SerializeField] public GTNodeGraphSO NodeGraph { get; set; }

        public void Initialize(GTNodeGraphSO nodeGraph)
        {
            Groups = new List<GTGroupData>();
            EditorNodes = new List<GTNodeEditorData>();
            NodeGraph = nodeGraph;
        }
    }
}