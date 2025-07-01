using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TEL.GraphTool.Utilities
{
    using Data;
    using Elements;
    using ScriptableObjects;
    using Windows;

    public static class GTIOUtility
    {
        // public static string AssetPath { get; private set; } = "Assets/Resources";
        public static string EditorAssetPath { get; private set; } = "Assets/Editor/Resources";

        private static GTGraphView graphView;

        private static List<GTNode> nodes = new();
        private static List<GTGroup> groups = new();

        private static Dictionary<string, GTGroup> loadedGroups = new();
        private static Dictionary<string, GTNode> loadedNodes = new();

        public static void Initialize(GTGraphView GTGraphView)
        {
            graphView = GTGraphView;

            nodes = new List<GTNode>();
            groups = new List<GTGroup>();

            loadedGroups = new Dictionary<string, GTGroup>();
            loadedNodes = new Dictionary<string, GTNode>();

            /*var settings = LoadSettings();
            if (settings != null)
            {
                AssetPath = settings.DefaultAssetFolder;
                EditorAssetPath = settings.DefaultEditorAssetFolder;
            }*/
        }

        public static GTNodeGraphSO CreateAsset(string pathname)
        {
            Save(pathname, out var nodeGraphSO);
            return nodeGraphSO;
        }

        public static GTNodeGraphSO CreateAsset(string pathname, GTComponentNode[] nodes)
        {
            var nodeDatas = GTSceneGrabber.GetStandardNodeDatas(nodes);
            var editorNodes = GTSceneGrabber.CreateStandardNodeEditorDatas(nodeDatas);
            var editorGroups = new List<GTGroupData>();
            return CreateAsset(pathname, nodeDatas, editorGroups, editorNodes);
        }

        public static GTNodeGraphSO CreateAsset(string pathname, List<GTNodeData> nodeData,
            List<GTGroupData> groupData, List<GTNodeEditorData> nodeEditorData)
        {
            Save(pathname, out var nodeGraphSO, (graphData, editorGraphData) =>
            {
                graphData.Nodes = nodeData;
                editorGraphData.Groups = groupData;
                editorGraphData.EditorNodes = nodeEditorData;
            });
            return nodeGraphSO;
        }

        public static void Save(string pathname)
        {
            Save(pathname, out var nodeGraphSO);
        }

        public static void Save(string pathname, out GTNodeGraphSO graphData)
        {
            Save(pathname, out graphData, (graphData, editorGraphData) =>
            {
                SaveGraphData(graphData);
                SaveEditorGraphData(editorGraphData);
            });
        }

        private static void Save(string pathname, out GTNodeGraphSO graphData,
            System.Action<GTNodeGraphSO, GTEditorGraphDataSO> fillAction)
        {
            var relativePath = GetRelativePath(pathname);
            var directory = Path.GetDirectoryName(relativePath);
            var filename = Path.GetFileNameWithoutExtension(pathname);

            GetElementsFromGraphView();
            graphData = CreateAsset<GTNodeGraphSO>(directory, $"{filename}_graph");

            graphData.Initialize();

            GTEditorGraphDataSO editorGraphData = CreateAsset<GTEditorGraphDataSO>(directory, $"{filename}");

            editorGraphData.Initialize(graphData);

            fillAction?.Invoke(graphData, editorGraphData);

            SaveAsset(graphData);
            SaveAsset(editorGraphData);
        }

        private static void SaveGraphData(GTNodeGraphSO graphData)
        {
            var graphNodes = nodes?.Select(node => new GTNodeData()
            {
                ID = node.ID,
                SceneID = node.SceneID,
                Name = node.NodeName,
                Level = node.Level,
                Links = CloneNodeLinks(node.Links)
            }).ToList();

            graphData.Nodes = graphNodes;
        }

        private static void SaveEditorGraphData(GTEditorGraphDataSO graphData)
        {
            var graphGroups = groups?.Select(group => new GTGroupData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            }).ToList();

            var graphEditorNodes = nodes?.Select(node => new GTNodeEditorData()
            {
                NodeID = node.ID,
                GroupID = node.Group?.ID,
                Position = node.GetPosition().position
            }).ToList();

            graphData.Groups = graphGroups;
            graphData.EditorNodes = graphEditorNodes;
        }

        public static bool Load(string filename, GTNodeGraphSO nodeGraphSO)
        {
            var relativePath = GetRelativePath(filename);

            GTEditorGraphDataSO graphData = LoadAsset<GTEditorGraphDataSO>(relativePath);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return false;
            }

            if (nodeGraphSO != null && nodeGraphSO != graphData.NodeGraph)
            {
                EditorUtility.DisplayDialog(
                    "Could not load the graph!",
                    "If the editor is running, use the same graph like on the scene.",
                    "Okay!"
                );

                return false;
            }

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.NodeGraph.Nodes, graphData.EditorNodes);
            LoadNodeLinks();

            return true;
        }

        private static string GetRelativePath(string filename)
        {
            var directory = Directory.GetCurrentDirectory();
            // return Path.GetRelativePath(directory, filename);
            return filename.Substring(directory.Length + 1);
        }

        private static void LoadGroups(List<GTGroupData> groups)
        {
            foreach (GTGroupData groupData in groups)
            {
                GTGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<GTNodeData> nodes, List<GTNodeEditorData> editorNodes)
        {
            var editorDictionary = editorNodes.ToDictionary(editorNode => editorNode.NodeID, editorNode => editorNode);

            foreach (GTNodeData nodeData in nodes)
            {
                var editorNode = editorDictionary[nodeData.ID];

                var initializeData = new GTNodeInitializeData()
                {
                    ID = nodeData.ID,
                    SceneID = nodeData.SceneID,
                    Name = nodeData.Name,
                    Level = nodeData.Level,
                    UnlockState = Enumerations.GTUnlockState.Locked,
                    Links = CloneNodeLinks(nodeData.Links),
                    Settings = nodeData.Settings,
                };

                GTNode node = graphView.CreateNode(initializeData, editorNode.Position);

                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(editorNode.GroupID))
                {
                    continue;
                }

                GTGroup group = loadedGroups[editorNode.GroupID];

                node.Group = group;

                group.AddElement(node);
            }
        }

        private static void LoadNodeLinks()
        {
            foreach (KeyValuePair<string, GTNode> loadedNode in loadedNodes)
            {
                var nodes = loadedNode.Value.outputContainer.Children();
                var preNodes = loadedNode.Value.inputContainer.Children().OfType<Port>().Where(port => port.direction == Direction.Output);
                var ports = nodes.Concat(preNodes);

                foreach (Port linkPort in ports)
                {
                    GTLinkData linkData = (GTLinkData)linkPort.userData;

                    if (string.IsNullOrEmpty(linkData.NodeID))
                    {
                        continue;
                    }

                    GTNode nextNode = loadedNodes[linkData.NodeID];

                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = linkPort.ConnectTo(nextNodeInputPort);

                    graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void GetElementsFromGraphView()
        {
            graphView?.graphElements.ForEach(graphElement =>
            {
                if (graphElement is GTNode node)
                {
                    nodes.Add(node);

                    return;
                }

                if (graphElement is GTGroup group)
                {
                    groups.Add(group);

                    return;
                }
            });
        }

        public static void CreateFolder(string parentFolderPath, string newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            return CreateAsset<T>(fullPath);
        }

        public static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = LoadAsset<T>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        public static T LoadAsset<T>(string filename) where T : ScriptableObject
        {
            return AssetDatabase.LoadAssetAtPath<T>(filename);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static List<GTLinkData> CloneNodeLinks(List<GTLinkData> nodeLinks)
        {
            return nodeLinks.Where(linkData => !string.IsNullOrEmpty(linkData.NodeID)).Select(linkData => linkData.Clone()).ToList();
        }

        /*private static GTSettings LoadSettings()
        {
            var foundAssets = AssetDatabase.FindAssets("t:GDL.GT.GTSettings");

            if (foundAssets.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
                return GTIOUtility.LoadAsset<GTSettings>(path);
            }

            return null;
        }*/
    }
}