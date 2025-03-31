using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEL.GraphTool.Utilities
{
    using Data;
    using Enumerations;
    using TEL.GraphTool.Elements;
    using Windows;

    public static class GTSceneGrabber
    {
        public static List<GTNodeInitializeData> GetNodesFromScene()
        {
            GTComponentNode[] nodes = null;

            if (PrefabStageUtility.GetCurrentPrefabStage() != null) //In Prefab Mode
                nodes = PrefabStageUtility.GetCurrentPrefabStage().FindComponentsOfType<GTComponentNode>();
            else
                nodes = UnityEngine.Object.FindObjectsOfType<GTComponentNode>(true);

            var nodeDatas = new List<GTNodeInitializeData>();

            foreach (var node in nodes)
            {
                for (int i = 1; i <= node.NumberOfLevels; ++i)
                {
                    var nodeData = new GTNodeInitializeData()
                    {
                        ID = $"{node.NodeID}_{i}",
                        SceneID = node.NodeID,
                        Name = node.gameObject.name,
                        Level = i,
                        UnlockState = GetUnlockState(node, i),
                        Settings = new GTNodeSettings()
                        {
                            IsAutoUnlockable = node.IsAutoUnlockable,
                        },
                        SceneNode = node,
                    };

                    nodeDatas.Add(nodeData);
                }
            }

            return nodeDatas;
        }

        public static List<GTNodeData> GetStandardNodeDatas(GTComponentNode[] nodes)
        {
            var nodeDatas = new List<GTNodeData>();

            foreach (var node in nodes)
            {
                for (int i = 1; i <= node.NumberOfLevels; ++i)
                {
                    var nodeData = new GTNodeData()
                    {
                        ID = $"{node.NodeID}_{i}",
                        SceneID = node.NodeID,
                        Name = node.gameObject.name,
                        Level = i,
                        Settings = new GTNodeSettings()
                        {
                            IsAutoUnlockable = node.IsAutoUnlockable,
                        },
                        Links = new()
                    };

                    nodeDatas.Add(nodeData);
                }
            }

            AddInheritanceIds(nodeDatas);

            return nodeDatas;
        }

        // we suppose that nodeDatas are stored in the right sequence.
        private static void AddInheritanceIds(List<GTNodeData> nodeDatas)
        {
            GTNodeData previousNode = null;

            foreach (var nodeData in nodeDatas)
            {
                if (previousNode?.SceneID == nodeData.SceneID)
                {
                    previousNode.Links.Add(new GTLinkData { Text = "Next", LinkType = GTLinkType.Regular, NodeID = nodeData.ID });
                }

                previousNode = nodeData;
            }
        }

        public static List<GTNodeEditorData> CreateStandardNodeEditorDatas(List<GTNodeData> nodeDatas)
        {
            var editorData = new List<GTNodeEditorData>();
            GTNodeData previousNode = null;
            float x = 200.0f, y = 0.0f;

            foreach (var nodeData in nodeDatas)
            {
                var newLine = previousNode?.SceneID != nodeData.SceneID;

                x = newLine ? 200.0f : x + 400.0f;
                y = newLine ? y + 200.0f : y;

                editorData.Add(new GTNodeEditorData()
                {
                    NodeID = nodeData.ID,
                    GroupID = null,
                    Position = new Vector2(x, y),
                });

                previousNode = nodeData;
            }

            return editorData;
        }

        private static GTUnlockState GetUnlockState(GTComponentNode node, int level)
        {
            /*if (!Application.isPlaying)
            {
                return GTUnlockState.Locked;
            }*/

            bool isUnlocked = node.CurrentLevel >= level;
            bool isUnlockable = node.IsUnlockable && (node.CurrentLevel + 1 == level);

            return isUnlocked ? GTUnlockState.Unlocked : isUnlockable ? GTUnlockState.Unlockable : GTUnlockState.Locked;
        }
    }
}