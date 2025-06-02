using System;
using UnityEngine;

namespace TEL.GraphTool.Data
{
    [Serializable]
    public class GTGroupData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}