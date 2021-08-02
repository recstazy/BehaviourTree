using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal class NodeData
    {
        #region Fields

        [SerializeReference]
        private BehaviourTask _taskImplementation;

        [SerializeField]
        private Vector2 _position;

        [SerializeField]
        private int _index;

        [SerializeField]
        private TaskConnection[] _connections;

        #endregion

        #region Properties

        public BehaviourTask TaskImplementation { get => _taskImplementation; internal set => _taskImplementation = value; }
        public int Index => _index;
        internal TaskConnection[] Connections { get => _connections; set => _connections = value; }
        internal Vector2 Position { get => _position; set => _position = value; }

        #endregion

        public NodeData()
        {
            Connections = new TaskConnection[0];
        }

        public NodeData(int index, BehaviourTask taskImlpementation, params TaskConnection[] connections)
        {
            _index = index;
            TaskImplementation = taskImlpementation;
            SetConnections(connections);
        }

        public NodeData CreateCopy(bool copyTask)
        {
            var data = new NodeData(_index, TaskImplementation, Connections);

            if (copyTask)
            {
                data.TaskImplementation = TaskImplementation?.CreateShallowCopy();
            }
            
            data.Position = Position;
            return data;
        }

        public void OverrideIndex(int newIndex)
        {
            _index = newIndex;
        }

        public void SetConnections(params TaskConnection[] connections)
        {
            if (connections == null)
            {
                Connections = new TaskConnection[0];
            }
            else
            {
                Connections = connections;
            }
        }

        [RuntimeInstanced]
        internal void CreateRuntimeConnections(NodeData[] treeData)
        {
            if (TaskImplementation != null)
            {
                List<BehaviourTask> connections = new List<BehaviourTask>(Connections.Length);
                int maxConnectionPinIndex = Connections.Length > 0 ? Connections.Max(c => c.OutPin) : -1;
                int connectionsTotalCount = maxConnectionPinIndex + 1;

                for (int i = 0; i < connectionsTotalCount; i++)
                {
                    connections.Add(null);
                }

                for (int i = 0; i < Connections.Length; i++)
                {
                    var nodeWithIndex = treeData.FirstOrDefault(d => d.Index == Connections[i].InNode);

                    if (nodeWithIndex != null)
                    {
                        connections[Connections[i].OutPin] = nodeWithIndex?.TaskImplementation;
                    }
                }

                TaskImplementation.SetRuntimeConnections(connections);
            }
        }
    }

    [System.Serializable]
    internal class TreeNodeData
    {
        [SerializeField]
        private NodeData[] data;

        public NodeData[] Data { get => data; internal set => data = value; }

        public TreeNodeData() 
        {
            Data = new NodeData[0];
        }

        public TreeNodeData(NodeData[] data)
        {
            Data = data;
        }
    }
}
