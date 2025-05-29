using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEL.GraphTool.Elements
{
    using Data;
    using Enumerations;
    using Utilities;
    using Windows;

    public class GTNode : Node
    {
        public string ID { get; set; }
        public string SceneID { get; set; }
        public string NodeName { get; set; }
        public int Level { get; set; }
        public GTUnlockState UnlockState { get; set; }
        public List<GTLinkData> Links { get; set; }
        public string Text { get; set; }
        public GTGroup Group { get; set; }
        public GTNodeSettings Settings { get; set; }

        public GTComponentNode SceneNode { get; set; } = null;

        protected GTGraphView graphView;
        private Color defaultBackgroundColor;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(GTNodeInitializeData nodeData, GTGraphView dsGraphView, Vector2 position)
        {
            ID = nodeData.ID;
            SceneID = nodeData.SceneID;
            NodeName = nodeData.Name;
            Level = nodeData.Level;
            UnlockState = nodeData.UnlockState;
            Links = nodeData.Links != null ? nodeData.Links : new List<GTLinkData>();
            Settings = nodeData.Settings;
            SceneNode = nodeData.SceneNode;

            SetPosition(new Rect(position, Vector2.zero));

            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public void Initialize(string nodeName, GTGraphView dsGraphView, Vector2 position)
        {
            var initializeData = new GTNodeInitializeData()
            {
                ID = Guid.NewGuid().ToString(),
                SceneID = "",
                Name = nodeName,
                Level = 0,
                Links = new List<GTLinkData>(),
                UnlockState = GTUnlockState.Locked,
                Settings = new GTNodeSettings()
                {
                    IsAutoUnlockable = false,
                }
            };

            Initialize(initializeData, dsGraphView, position);
        }

        public void UpdateData(GTNodeInitializeData nodeData)
        {
            NodeName = nodeData.Name;
            UnlockState = nodeData.UnlockState;
            Settings = nodeData.Settings;
            SceneNode = nodeData.SceneNode;
            UpdateLockedStateStyles();

            DrawExtension();
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */
            var nodeName = NodeName + " Lvl: " + Level;

            Label headerLabel = GTElementUtility.CreateLabel(nodeName);

            headerLabel.AddClasses(
                "ds-node__label-field"
            );
            headerLabel.style.fontSize = 12;

            titleContainer.Insert(0, headerLabel);

            DrawExtension();

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("From", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);
        }

        public virtual void DrawExtension()
        {
            extensionContainer.Clear();

            var foldout = GTElementUtility.CreateFoldout("Settings", true);
            extensionContainer.Add(foldout);

            var isAutoUnlockable = GTElementUtility.CreateToggle("Auto unlockable", Settings.IsAutoUnlockable, (changeEvent) =>
            {
                Settings.IsAutoUnlockable = changeEvent.newValue;

                if (SceneNode != null)
                {
                    SceneNode.IsAutoUnlockable = changeEvent.newValue;
                }
            });
            foldout.Add(isAutoUnlockable);
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = GTColors.LockedNodeColor;
        }

        private void UpdateLockedStateStyles()
        {
            var color = GetLockedStateColor();

            mainContainer.style.backgroundColor = color;

            foreach (Port port in outputContainer.Children())
            {
                foreach (Edge edge in port.connections)
                {
                    edge.style.backgroundColor = color;
                }
            }
        }

        private Color GetLockedStateColor()
        {
            switch (UnlockState)
            {
                case GTUnlockState.Unlocked:
                    {
                        return GTColors.UnlockedNodeColor;
                    }
                case GTUnlockState.Unlockable:
                    {
                        return GTColors.UnlockableNodeColor;
                    }
                default:
                    {
                        return GTColors.LockedNodeColor;
                    }
            }
        }
    }
}