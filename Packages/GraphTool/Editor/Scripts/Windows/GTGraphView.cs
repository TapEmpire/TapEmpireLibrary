using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEL.GraphTool.Windows
{
    using Data;
    using Elements;
    using Utilities;

    public class GTGraphView : GraphView
    {
        private GTEditorWindow editorWindow;
        private GTSearchWindow searchWindow;
        private MiniMap miniMap;

        public GTGraphView(GTEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;

            AddManipulators();
            AddGridBackground();
            // AddSearchWindow();
            AddMiniMap();

            OnElementsDeleted();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                GTNode startNode = (GTNode)startPort.node;
                GTNode targetNode = (GTNode)port.node;
                if (startNode.Links.Any(link => link.NodeID == targetNode.ID))
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("NodeGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }

        public GTGroup CreateGroup(string title, Vector2 position)
        {
            GTGroup group = new GTGroup(title, position);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is GTNode))
                {
                    continue;
                }

                GTNode node = (GTNode)selectedElement;

                group.AddElement(node);
            }

            return group;
        }

        public GTNode CreateNode(GTNodeInitializeData nodeData, Vector2 position)
        {
            GTNode node = new GTMultipleLinkNode();
            node.Initialize(nodeData, this, position);
            node.UpdateData(nodeData);
            node.Draw();

            return node;
        }

        public GTNode CreateNode(string nodeName, Vector2 position, bool shouldDraw = true)
        {
            GTNode node = new GTMultipleLinkNode();

            node.Initialize(nodeName, this, position);
            node.Level = 0;

            if (shouldDraw)
            {
                node.Draw();
            }

            return node;
        }

        public void UpdateNodesFromScene()
        {
            var nodeDatas = GTSceneGrabber.GetNodesFromScene();
            
           
            
            var nodes = graphElements.ToList()
                .Where(graphElement => graphElement is GTNode node)
                .Select(graphElement => (GTNode)graphElement);

            var currentNodes = nodes.Where(node => nodeDatas.Any(nodeData => nodeData.ID == node.ID));
            var absentNodes = nodeDatas.Where(nodeData => !nodes.Any(node => node.ID == nodeData.ID));
            var unusedNodes = nodes.Where(node => !nodeDatas.Any(nodeData => nodeData.ID == node.ID));

            foreach (var node in unusedNodes)
            {
                this.RemoveNode(node);
            }

            foreach (var node in absentNodes)
            {
                var graphNode = CreateNode(node, GetLocalMousePosition(new Vector2(200.0f, 200.0f)));

                AddElement(graphNode);
            }

            foreach (var node in currentNodes)
            {
                var nodeData = nodeDatas.Find(nodeData => nodeData.ID == node.ID);
                if (nodeData != null)
                {
                    node.UpdateData(nodeData);
                }
            }
        }

        /// TODO: Should be optimized.
        public void UpdateNodesFromScene(List<string> nodeIDs)
        {
            var nodeDatas = GTSceneGrabber.GetNodesFromScene();
            var nodes = graphElements.ToList()
                .Where(graphElement => graphElement is GTNode node)
                .Select(graphElement => (GTNode)graphElement).ToList();

            var currentNodes = nodes.Where(node => nodeDatas.Any(nodeData => nodeData.ID == node.ID));

            foreach (var node in currentNodes)
            {
                var nodeData = nodeDatas.Find(nodeData => nodeData.ID == node.ID);
                if (nodeData != null)
                {
                    node.UpdateData(nodeData);
                }
            }
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(GTGroup);
                Type edgeType = typeof(Edge);

                List<GTGroup> groupsToDelete = new List<GTGroup>();
                List<GTNode> nodesToDelete = new List<GTNode>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach (GraphElement selectedElement in selection)
                {
                    if (selectedElement is GTNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if (selectedElement.GetType() == edgeType)
                    {
                        Edge edge = (Edge)selectedElement;

                        edgesToDelete.Add(edge);

                        continue;
                    }

                    if (selectedElement.GetType() != groupType)
                    {
                        continue;
                    }

                    GTGroup group = (GTGroup)selectedElement;

                    groupsToDelete.Add(group);
                }

                foreach (GTGroup groupToDelete in groupsToDelete)
                {
                    List<GTNode> groupNodes = new List<GTNode>();

                    foreach (GraphElement groupElement in groupToDelete.containedElements)
                    {
                        if (!(groupElement is GTNode))
                        {
                            continue;
                        }

                        GTNode groupNode = (GTNode)groupElement;

                        groupNodes.Add(groupNode);
                    }

                    groupToDelete.RemoveElements(groupNodes);

                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                foreach (GTNode nodeToDelete in nodesToDelete)
                {
                    if (nodeToDelete.Group != null)
                    {
                        nodeToDelete.Group.RemoveElement(nodeToDelete);
                    }

                    nodeToDelete.DisconnectAllPorts();

                    RemoveElement(nodeToDelete);
                }
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        GTNode nextNode = (GTNode)edge.input.node;

                        GTLinkData linkData = (GTLinkData)edge.output.userData;

                        linkData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge)element;

                        GTLinkData linkData = (GTLinkData)edge.output.userData;

                        linkData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        private void RemoveNode(GTNode node)
        {
            if (node.Group != null)
            {
                node.Group.RemoveElement(node);
            }

            node.DisconnectAllPorts();
            RemoveElement(node);
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "GTGraphViewStyles",
                "GTNodeStyles"
            );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
    }
}