using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TapEmpire.Utility;

namespace TEL.GraphTool
{
    using Data;
    using ScriptableObjects;

    public class GTComponentGraph : MonoBehaviour
    {
        /// Action with list of unlocked nodes for Editor (Editor NodeID used).
        public System.Action<List<string>> OnNodesUpdated = null;
        public System.Action<GTComponentNode> OnNodeUnlockedAction = null;
        public System.Action OnInitialized = null;

        [field: SerializeField] public GTNodeGraphSO NodeGraph { get; set; }

        [Tooltip("If true, you have to unlock all previous node paths to unlock the next one")]
        [SerializeField] private bool useCombinedUnlock = true;

        [Tooltip("If true, the graph will be initialized on Awake, otherwise do it manually via Load method")]
        [SerializeField] private bool initializeOnAwake = false;

        [Header("Simulation")]
        [SerializeField] private bool enableSimulation = false;
        [SerializeField] private float unlockDelay = 1.0f;
        [SerializeField] private float startDelay = 3.0f;

        protected Dictionary<string, GTComponentNode> sceneNodes = null;
        private Dictionary<string, GTNodeData> graphNodes = null;
        private Dictionary<string, GTNodeData> fullGraphNodes = null;
        
        protected List<GTNodeData> _nodeDataList = null;

        private List<GTComponentNode> simulationQueue = null;

        public List<GTComponentNode> NodesToUnlock => simulationQueue;
        public List<GTNodeData> AllSceneNodes => _nodeDataList;

        public BoolCounter IsDebugUnlockOn { get; private set; } = new BoolCounter(false);

        private void Awake()
        {
            if (initializeOnAwake)
            {
                Load();
            }
        }

        public void Load(GTComponentNode[] nodes = null)
        {
            var componentNodes = nodes == null ? FindObjectsOfType<GTComponentNode>(true) : nodes;

            sceneNodes = componentNodes.ToDictionary(componentNode => componentNode.NodeID, componentNode => componentNode);
            graphNodes = NodeGraph.Nodes.ToDictionary(graphNode => graphNode.ID, graphNode => GTNodeData.CreateDynamicNodeData(graphNode));
            fullGraphNodes = graphNodes.Values.ToDictionary(graphNode => graphNode.ID, graphNode => graphNode);
            
            _nodeDataList = graphNodes.Values.ToList();

            var unlockableNodes = componentNodes.Where(componentNode => componentNode.IsUnlockable).ToList();
            // unlockableNodes.ForEach(node => node.OnUnlockAnimationFinished += OnNodeUnlocked);

            // TODO: Change Select to ForEach from extensions later.
            componentNodes.Where(node => !node.IsFullyUnlocked).ToList().ForEach(node => SubscribeToNode(node));

            Array.ForEach(componentNodes, node => node.SetInitialState());

            // Create reverse graph.
            foreach (var graphNode in graphNodes)
            {
                if (sceneNodes.ContainsKey(graphNode.Value.SceneID))
                {
                    var componentNode = sceneNodes[graphNode.Value.SceneID];
                    var isUnlocked = componentNode.IsUnlocked(graphNode.Value.Level);

                    if (!isUnlocked)
                    {
                        foreach (var link in graphNode.Value.Links)
                        {
                            var node = graphNodes[link.NodeID];
                            node.PreviousLinks.Add(graphNode.Key);
                            
                            if (link.LinkType == GTLinkType.PreUnlock)
                            {
                                graphNode.Value.PreUnlockLinks.Add(node.SceneID);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Can't find in sceneNodes {graphNode.Value.Name}, {graphNode.Value.SceneID}" );
                }
            }
            
            foreach (var graphNode in graphNodes)
            {
                var hasNoParents = graphNode.Value.PreviousLinks.Count == 0;

                if (hasNoParents)
                {
                    if (sceneNodes.ContainsKey(graphNode.Value.SceneID))
                    {
                        var componentNode = sceneNodes[graphNode.Value.SceneID];
                        componentNode.MakeUnlockable(graphNode.Value.Level);

                        if (componentNode.IsUnlockable)
                        {
                            // componentNode.OnUnlockAnimationFinished += OnNodeUnlocked;
                            unlockableNodes.Add(componentNode);
                        }
                    } else
                    {
                        Debug.LogError($"Can't find in sceneNodes {graphNode.Value.SceneID}" );
                    }
                }
            }

            simulationQueue = new List<GTComponentNode>(unlockableNodes);

            // Simulation
            if (enableSimulation)
            {
                Simulation();
            }
            
            OnInitialized?.Invoke();
        }

        private U GetValue<T, U>(Dictionary<T, U> dictionary, T key)
        {
            U value;
            bool success = dictionary.TryGetValue(key, out value);
            return success ? value : default(U);
        }
        
        private void OnNodeUnlockedInternal(GTComponentNode node)
        {
            OnNodeUnlocked(node).Forget();
        }
        
        protected virtual async UniTask OnNodeUnlocked(GTComponentNode node)
        {
            OnNodesUpdated?.Invoke(new List<string> { node.NodeID });
        }

        protected virtual void OnNodeMadeUnlockable(GTComponentNode node)
        {
            OnNodesUpdated?.Invoke(new List<string> { node.NodeID });
        }

        protected virtual void StartUnlockNode(GTComponentNode node, bool quick)
        {
            UnlockNode(node, quick).Forget();
        }
        
        // Should be renamed to PreUnlocks or something.
        // Preunlocks ignore the normal flow of unlocking! (triggers and callbacks)
        protected async UniTask UnlockNode(GTComponentNode node, bool quick)
        {
            if (graphNodes.TryGetValue(node.LeveledNodeId, out var graphNode))
            {
                foreach (var link in graphNode.PreUnlockLinks)
                {
                    await sceneNodes[link].UnlockWithoutPreNodes(quick);
                }
                /*MakeNodesUnlockable(graphNode.PreUnlockLinks.Select(link => (sceneNodes[link], graphNode.Level))
                    .ToList());

                foreach (var links in graphNode.PreUnlockLinks)
                {
                    await UnlockNode(sceneNodes[links], quick);
                }*/
            }

            await node.UnlockUnlockable(quick);
        }

        protected virtual void OnUnlockAnimationFinished(GTComponentNode node)
        {
            var graphID = GTNodeData.GeneratedID(node.NodeID, node.CurrentLevel);
            var unlockedNode = GetValue(graphNodes, graphID);

            simulationQueue.Remove(node);
            
            OnNodeUnlockedAction?.Invoke(node);

            if (unlockedNode != null)
            {
                var nodesMadeUnlockable = new List<(GTComponentNode, int)>();

                if (node.IsFullyUnlocked)
                {
                    UnsubscribeFromNode(node);
                }

                var unlockableIDs = unlockedNode.Links.Where(node => node.LinkType == GTLinkType.Regular).Select(node => node.NodeID);

                foreach (var unlockableID in unlockableIDs)
                {
                    var unlockableGraphNode = graphNodes[unlockableID];

                    var isNodeUnlockable = useCombinedUnlock ? unlockableGraphNode.UnlockPrevious(unlockedNode.ID) : true;

                    if (isNodeUnlockable)
                    {
                        var unlockableSceneNode = sceneNodes[unlockableGraphNode.SceneID];

                        if (!unlockableSceneNode.IsUnlocked(unlockableGraphNode.Level))
                        {
                            // unlockableSceneNode.OnUnlockAnimationFinished += OnNodeUnlocked;
                            // unlockableSceneNode.MakeUnlockable(unlockableGraphNode.Level);
                            nodesMadeUnlockable.Add((unlockableSceneNode, unlockableGraphNode.Level));

                            simulationQueue.Add(unlockableSceneNode);
                        }
                    }
                }

                MakeNodesUnlockable(nodesMadeUnlockable);
                // OnNodesUpdated?.Invoke(unlockableIDs.ToList());
            }
        }

        protected virtual void MakeNodesUnlockable(List<(GTComponentNode, int)> unlockableNodes)
        {
            unlockableNodes.ForEach(nodeData => nodeData.Item1.MakeUnlockable(nodeData.Item2));
        }

        public virtual void UnlockAllDebug()
        {
            IsDebugUnlockOn.SetValue(true);

            StartCoroutine(UnlockAllDebugCoroutine());
        }

        private IEnumerator UnlockAllDebugCoroutine()
        {
            var previousNode = String.Empty;
            while (simulationQueue.Count > 0)
            {
                var node = simulationQueue.First();

                if (previousNode == node.LeveledNodeId)
                {
                    simulationQueue.RemoveAt(0);
                }
                else
                {
                    previousNode = node.LeveledNodeId;
                    node.Unlock(true).Forget();

                    yield return new WaitForSeconds(0.1f);
                }
            }

            IsDebugUnlockOn.SetValue(false);
        }

        protected void UnlockNextNodes(string target)
        {
            IsDebugUnlockOn.SetValue(true);

            var queue = new Queue<GTNodeData>();
            var passedNodes = new Dictionary<string, GTNodeData>();
            var list = new List<GTNodeData>();
            queue.Enqueue(fullGraphNodes[target]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (passedNodes.ContainsKey(current.ID)) continue;
                    
                list.Add(current);
                passedNodes.Add(current.ID, current);
                for (int i = 0; i < current.PreviousLinks.Count; i++)
                {
                    var neighborNode = fullGraphNodes[current.PreviousLinks[i]];
                    if (sceneNodes[neighborNode.SceneID].IsUnlocked(current.Level)) continue;

                    queue.Enqueue(fullGraphNodes[neighborNode.ID]);
                }
            }

            passedNodes.Clear();
            list.Reverse();
            var tempSimulationQueue = list.Select(nodeData => sceneNodes[nodeData.SceneID]).ToList();
            UnlockNodeListAsQueue(tempSimulationQueue);
            
            TapEmpire.Utility.Utility.RestartScene();

            IsDebugUnlockOn.SetValue(false);
        }

        private void UnlockNodeListAsQueue(List<GTComponentNode> nodes)
        {
            while (nodes.Count > 0)
            {
                var node = nodes.First();
                //node.
                node.Unlock(true).Forget();
                nodes.RemoveAt(0);
            }
        }
        
        private void SubscribeToNode(GTComponentNode node)
        {
            node.OnStartUnlock += StartUnlockNode;
            node.OnUnlocked += OnNodeUnlockedInternal;
            node.OnUnlockAnimationFinished += OnUnlockAnimationFinished;
            node.OnMadeUnlockable += OnNodeMadeUnlockable;
        }

        private void UnsubscribeFromNode(GTComponentNode node)
        {
            node.OnStartUnlock -= StartUnlockNode;
            node.OnUnlocked -= OnNodeUnlockedInternal;
            node.OnUnlockAnimationFinished -= OnUnlockAnimationFinished;
            node.OnMadeUnlockable -= OnNodeMadeUnlockable;
        }

        private async void Simulation()
        {
            await Task.Delay(TimeSpan.FromSeconds(startDelay));
            while (simulationQueue.Count > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(unlockDelay));
                var node = simulationQueue.First();
                // simulationQueue.RemoveAt(0);
                node.Unlock(true).Forget();
            }
        }
    }
}