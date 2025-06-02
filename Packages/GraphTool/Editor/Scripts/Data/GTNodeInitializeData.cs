using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEL.GraphTool.Data
{
    using Enumerations;

    public class GTNodeInitializeData
    {
        public string ID { get; set; }
        public string SceneID { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public GTUnlockState UnlockState { get; set; }
        public List<GTLinkData> Links { get; set; }
        public string GroupID { get; set; }
        public Vector2 Position { get; set; }
        public GTNodeSettings Settings { get; set; }

        public GTComponentNode SceneNode { get; set; } = null; // original node on the scene, so that we can change data from GraphView.
    }
}