using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEL.GraphTool.Data
{
    [Serializable]
    public class GTNodeEditorData
    {
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}