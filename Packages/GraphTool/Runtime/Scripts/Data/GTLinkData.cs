using System;
using UnityEngine;

namespace TEL.GraphTool.Data
{
    public enum GTLinkType
    {
        Regular = 0,
        PreUnlock = 1,
    }

    [Serializable]
    public class GTLinkData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public GTLinkType LinkType { get; set; }

        public GTLinkData Clone()
        {
            return new GTLinkData() { Text = this.Text, NodeID = this.NodeID, LinkType = this.LinkType };
        }
    }
}