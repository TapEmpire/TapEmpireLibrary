using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEL.GraphTool.Data
{
    using Enumerations;

    [Serializable]
    public class GTNodeData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string SceneID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public int Level { get; set; }
        [field: SerializeField] public List<GTLinkData> Links { get; set; }
        [field: SerializeField] public GTNodeSettings Settings { get; set; }

        public List<string> PreviousLinks { get; set; } = new ();
        public HashSet<string> PreUnlockLinks { get; set; } = new ();
        
        public GTUnlockState UnlockState { get; set; }

        public bool UnlockPrevious(string previousID)
        {
            PreviousLinks.Remove(previousID);
            return PreviousLinks.Count == 0;
        }

        public static string GeneratedID(string sceneID, int level)
        {
            return $"{sceneID}_{level}";
        }

        public static GTNodeData CreateDynamicNodeData(GTNodeData nodeData)
        {
            var node = (GTNodeData) nodeData.MemberwiseClone();
            node.PreviousLinks = new List<string>();

            return node;
        }
    }
}