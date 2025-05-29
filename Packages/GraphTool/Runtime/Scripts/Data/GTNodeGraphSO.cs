using System.Collections.Generic;
using UnityEngine;

namespace TEL.GraphTool.ScriptableObjects
{
    using Data;
    using Sirenix.OdinInspector;

    public class GTNodeGraphSO : ScriptableObject
    {
        [field: SerializeField][field: Searchable] public List<GTNodeData> Nodes { get; set; }

        public void Initialize()
        {
            Nodes = new List<GTNodeData>();
        }
    }
}