using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNodeProcessor
    {
        #region Fields

        private MultioutNodeProcessor _multioutProcessor;

        #endregion

        #region Properties

        public List<BehaviourTreeNode> Nodes { get; private set; } = new List<BehaviourTreeNode>();
        public bool NodesDirty { get; private set; } = false;

        #endregion

        public void Dispose()
        {
            Nodes.Clear();
            NodesDirty = false;
            _multioutProcessor?.Dispose();
        }

        public void RecreateNodes(BehaviourTree tree)
        {
            _multioutProcessor?.Dispose();
            Nodes.Clear();
            var treeData = tree.NodeData;

            for (int i = 0; i < treeData.Data.Length; i++)
            {
                NodeData data = treeData.Data[i];
                CreateNodeAndAppendToList(data);
            }

            _multioutProcessor = new MultioutNodeProcessor(Nodes);
        }

        public void OnNodesGUI()
        {
            BehaviourTreeNode nodeToRecreate = null;

            foreach (var node in Nodes)
            {
                node.OnGUI();

                if (node.IsDirty)
                {
                    NodesDirty = true;
                    UpdateNodeAfterTaskChanged(node);
                    nodeToRecreate = node;
                    node.IsDirty = false;
                }
            }

            if (nodeToRecreate != null)
            {
                var newNode = CreateNodeFromData(nodeToRecreate.Data);
                UpdateNodeWithIndex(nodeToRecreate.Index, newNode);
            }
        }

        public void ClearNodesDirty()
        {
            NodesDirty = false;
        }

        public BehaviourTreeNode GetNode(int index)
        {
            return Nodes.FirstOrDefault(n => n.Index == index);
        }

        public void CreateNewNodeInEditor()
        {
            var nodeData = new NodeData(GetAvailableNodeIndex(), null, null);
            var centerPosition = BTEventProcessor.LastMousePosition;

            if (BTSnapManager.SnapEnabled)
            {
                centerPosition = BTSnapManager.RoundToSnap(centerPosition);
            }

            nodeData.Position = centerPosition;
            CreateNodeAndAppendToList(nodeData);
        }

        public BehaviourTreeNode CreateNodeFromData(NodeData data)
        {
            var node = NodeDrawerProvider.GetDrawerForData(data);
            return node;
        }

        public void CreateNodeAndAppendToList(NodeData data)
        {
            var node = CreateNodeFromData(data);
            Nodes.Add(node);
        }

        public bool DeleteNodesBySelection(HashSet<int> listiIndices)
        {
            if (listiIndices.Count > 0)
            {
                var nodeIndices = listiIndices.Select(i => Nodes[i].Index).ToArray();

                foreach (var i in nodeIndices)
                {
                    DeleteNodeWithIndex(i);
                }

                return true;
            }

            return false;
        }

        public void CreateConnection(NodeConnectionEventArgs args)
        {
            var outNode = GetNode(args.OutNode);
            var newData = outNode.Data.CreateCopy(false);
            var newConnections = CreateNewConnection(newData.Connections, args.OutPin, args.InNode);
            _multioutProcessor.AfterConnectionAdded(newData, newConnections);
            newData.SetConnections(newConnections.ToArray());
            outNode = CreateNodeFromData(newData);
            UpdateNodeWithIndex(outNode.Index, outNode);
        }

        public void RemoveConnection(NodeConnectionEventArgs args)
        {
            var outNode = GetNode(args.OutNode);
            var newData = outNode.Data.CreateCopy(false);
            var newConnections = RemoveExistingConnection(newData.Connections, args.OutPin);
            _multioutProcessor.AfterConnectionRemoved(newData, newConnections);
            newData.SetConnections(newConnections.ToArray());
            outNode = CreateNodeFromData(newData);
            UpdateNodeWithIndex(outNode.Index, outNode);
        }

        public bool AfterNodesDragged(HashSet<int> indices)
        {
            bool anyChanged = false;

            foreach (var i in indices)
            {
                if (AfterNodeDragged(i))
                {
                    anyChanged = true;
                }
            }

            return anyChanged;
        }

        public bool UpdateRunningNodesAndCheckChanged()
        {
            bool changed = false;

            foreach (var n in Nodes)
            {
                if (n.RuntimeIsRunningChanged)
                {
                    changed = true;
                    break;
                }
            }

            return changed;
        }

        private bool AfterNodeDragged(int listIndex)
        {
            var node = Nodes[listIndex];
            bool nodesChanged = _multioutProcessor.AfterNodePositionChanged(node, out var nodesIndices);

            if (nodesChanged)
            {
                foreach (var i in nodesIndices)
                {
                    var changedNode = GetNode(i);
                    var newData = changedNode.Data.CreateCopy(false);
                    var newNode = CreateNodeFromData(newData);
                    UpdateNodeWithIndex(changedNode.Index, newNode);
                }
            }

            return nodesChanged;
        }

        public void CopySelectedToClipboard(HashSet<int> listIndices)
        {
            if (listIndices is null || listIndices.Count == 0) return;
            NodeCopier.Copy(listIndices.Select(i => Nodes[i].Data).ToArray());
        }

        public bool DuplicateNodes(HashSet<int> listIndices)
        {
            if (listIndices is null || listIndices.Count == 0) return false;
            CopySelectedToClipboard(listIndices);
            return CreateNewNodesFromClipboard();
        }

        public bool CreateNewNodesFromClipboardUnderMouse()
        {
            return CreateNewNodesFromClipboard();
        }

        public bool CreateNewNodesFromClipboard()
        {
            int availableIndex = GetAvailableNodeIndex();
            var newData = NodeCopier.GetCopiedNodes(availableIndex, Vector2.zero);
            if (newData is null) return false;

            foreach (var data in newData)
            {
                if (data.TaskImplementation is MultioutTask)
                {
                    _multioutProcessor.UpdateMultioutConnections(data);
                }

                CreateNodeAndAppendToList(data);
            }

            return true;
        }

        private void DeleteNodeWithIndex(int nodeIndex)
        {
            if (BTNodeInspector.CurrentNodeIndex == nodeIndex)
            {
                BTNodeInspector.CloseInspector();
            }

            RemoveAllOutsToNode(nodeIndex);
            RemoveNodeAtIndex(nodeIndex);
        }

        private void RemoveAllOutsToNode(int nodeIndex)
        {
            var nodesOutedToThis = Nodes
                .Where(n => n.Connections.Select(c => c.InNode)
                .Contains(nodeIndex));

            List<NodeConnectionEventArgs> removalShedule = new List<NodeConnectionEventArgs>();

            foreach (var outNode in nodesOutedToThis)
            {
                foreach (var c in outNode.Connections)
                {
                    if (c.InNode == nodeIndex)
                    {
                        removalShedule.Add(new NodeConnectionEventArgs(outNode.Index, c.OutPin, 0));
                    }
                }
            }

            foreach (var removalArgs in removalShedule)
            {
                RemoveConnection(removalArgs);
            }
        }

        private List<TaskConnection> CreateNewConnection(TaskConnection[] connections, int outIndex, int inNodeIndex)
        {
            var list = connections.ToList();
            list.Add(new TaskConnection(outIndex, inNodeIndex));
            return list;
        }

        private List<TaskConnection> RemoveExistingConnection(TaskConnection[] connections, int outIndex)
        {
            int connectionIndex = -1;

            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].OutPin == outIndex)
                {
                    connectionIndex = i;
                    break;
                }
            }

            if (connectionIndex >= 0)
            {
                var list = connections.ToList();
                list.RemoveAt(connectionIndex);
                return list;
            }

            return connections.ToList();
        }

        private void RemoveNodeAtIndex(int index)
        {
            int listIndex = GetListIndex(index);

            if (listIndex > 0)
            {
                Nodes.RemoveAt(listIndex);
            }
        }

        private void UpdateNodeWithIndex(int nodeIndex, BehaviourTreeNode newNode)
        {
            var listIndex = GetListIndex(nodeIndex);

            if (listIndex >= 0)
            {
                Nodes[listIndex] = newNode;
            }
        }

        private int GetListIndex(int nodeIndex)
        {
            return Nodes.IndexOf(Nodes.FirstOrDefault(n => n.Index == nodeIndex));
        }

        private bool UpdateNodeAfterTaskChanged(BehaviourTreeNode node)
        {
            if (node.Data.TaskImplementation is MultioutTask)
            {
                _multioutProcessor.UpdateMultioutConnections(node.Data);
                return true;
            }
            else
            {
                var outs = node.GetOuts();

                if (outs is null)
                {
                    outs = new TaskOutAttribute[0];
                }

                if (node.Connections.Length > 0)
                {
                    int maxPinIndex = node.Connections.Max(t => t.OutPin);

                    if (maxPinIndex >= outs.Length)
                    {
                        var newConnections = new List<TaskConnection>();

                        for (int i = 0; i < outs.Length && i < node.Connections.Length; i++)
                        {
                            var connection = node.Connections.FirstOrDefault(c => c.OutPin == i);

                            if (connection.IsValid)
                            {
                                newConnections.Add(connection);
                            }
                        }

                        node.Data.SetConnections(newConnections.ToArray());
                        return true;
                    }
                }

                return false;
            }
        }

        private int GetAvailableNodeIndex()
        {
            if (Nodes.Count > 0)
            {
                return Nodes.Max(n => n.Index) + 1;
            }

            return 0;
        }
    }
}
