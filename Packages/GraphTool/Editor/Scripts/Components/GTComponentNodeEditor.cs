using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TEL.GraphTool
{
    [CustomEditor(typeof(GTComponentNode), true)]
    public class GTComponentNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var component = (GTComponentNode)target;
            
            base.OnInspectorGUI();

            if (GUILayout.Button("Reset NodeID"))
            {
                component.ResetNodeID();
            }
        }
    }
}