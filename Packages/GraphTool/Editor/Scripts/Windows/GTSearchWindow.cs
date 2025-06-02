using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TEL.GraphTool.Windows
{
    using Elements;

    public enum CreationType
    {
        MultipleNode,
        Group
    }

    public class GTSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private GTGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(GTGraphView dsGraphView)
        {
            graphView = dsGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
                new SearchTreeEntry(new GUIContent("Multiple Node", indentationIcon))
                {
                    userData = CreationType.MultipleNode,
                    level = 1
                },
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = CreationType.Group,
                    level = 1
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case CreationType.MultipleNode:
                {
                    GTMultipleLinkNode multipleLinkNode = (GTMultipleLinkNode) graphView.CreateNode("NodeName", localMousePosition);

                    graphView.AddElement(multipleLinkNode);

                    return true;
                }

                case CreationType.Group:
                {
                    graphView.CreateGroup("NodeGroup", localMousePosition);

                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}