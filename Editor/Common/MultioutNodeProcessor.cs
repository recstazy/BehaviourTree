using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class MultioutNodeProcessor
    {
        #region Fields

        private List<BehaviourTreeNode> _nodes;
        private HashSet<BehaviourTreeNode> _multioutNodes;

        #endregion

        public MultioutNodeProcessor(List<BehaviourTreeNode> nodes)
        {
            _nodes = nodes;
            _multioutNodes = new HashSet<BehaviourTreeNode>(nodes.Where(n => n.Data.TaskImplementation is MultioutTask));
        }

        public void Dispose()
        {
            _multioutNodes = null;
            _nodes = null;
        }

        public bool AfterConnectionAdded(NodeData data, List<TaskConnection> afterAdd)
        {
            if (data.TaskImplementation is MultioutTask multioutTask)
            {
                ProcessMultiout(multioutTask, afterAdd);
                return true;
            }

            return false;
        }

        public bool AfterConnectionRemoved(NodeData data, List<TaskConnection> afterRemove)
        {
            if (data.TaskImplementation is MultioutTask multioutTask)
            {
                ProcessMultiout(multioutTask, afterRemove);
                return true;
            }

            return false;
        }

        public bool AfterNodePositionChanged(BehaviourTreeNode node, out int[] nodesChanged)
        {
            if (_multioutNodes.Count > 0)
            {
                var outedToThisNode = _multioutNodes.Where(n => n.Connections.Where(c => c.InNode == node.Index).Count() > 0).ToArray();

                if (outedToThisNode.Length > 0)
                {
                    bool[] changedFlags = new bool[outedToThisNode.Length];

                    for (int i = 0; i < outedToThisNode.Length; i++)
                    {
                        var outedNode = outedToThisNode[i];
                        var connections = outedNode.Data.Connections.ToList();

                        if (RearrangeByNodePositionX(connections))
                        {
                            changedFlags[i] = true;
                            outedNode.Data.SetConnections(connections.ToArray());
                        }
                    }

                    nodesChanged = changedFlags
                        .Where(flag => flag == true)
                        .Select((flag, i) => outedToThisNode[i].Index)
                        .ToArray();

                    return nodesChanged.Length > 0;
                }
            }

            nodesChanged = null;
            return false;
        }

        public void UpdateMultioutConnections(NodeData newData)
        {
            var connections = newData.Connections.ToList();
            ProcessMultiout(newData.TaskImplementation as MultioutTask, connections);
            newData.SetConnections(connections.ToArray());
        }

        private void ProcessMultiout(MultioutTask multioutTask, List<TaskConnection> connections)
        {
            multioutTask.ChangeOutsCount(connections.Count);
            RearrangeByNodePositionX(connections);
        }

        private bool RearrangeByNodePositionX(List<TaskConnection> connections)
        {
            bool anyConnectionChanged = false;
            connections.Sort(SortByNodeXComparison);

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].OutPin != i)
                {
                    anyConnectionChanged = true;
                    connections[i] = new TaskConnection(i, connections[i].InNode);
                }
            }

            return anyConnectionChanged;
        }

        private int SortByNodeXComparison(TaskConnection first, TaskConnection second)
        {
            var firstPosX = _nodes.FirstOrDefault(n => n.Index == first.InNode)?.MainRect.position.x;
            var secondPosX = _nodes.FirstOrDefault(n => n.Index == second.InNode)?.MainRect.position.x;

            if (firstPosX.HasValue && secondPosX.HasValue)
            {
                if (firstPosX.Value < secondPosX.Value)
                {
                    return -1;
                }
                else if (firstPosX.Value > secondPosX.Value)
                {
                    return 1;
                }
                else return 0;
            }

            return -1;
        }
    }
}
