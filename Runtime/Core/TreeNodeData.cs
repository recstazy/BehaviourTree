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

        [SerializeField]
        private Vector2 _position;

        [SerializeReference]
        private BehaviourTask _taskImplementation;

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
            if (connections == null) Connections = new TaskConnection[0];
            else Connections = connections;
        }

        public bool TryFindIndexOfConnection(int outPin, int inNodeIndex, out int connectionIndex)
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                var c = _connections[i];

                if (c.OutPin == outPin && c.InNode == inNodeIndex)
                {
                    connectionIndex = i;
                    return true;
                }
            }

            connectionIndex = -1;
            return false;
        }

        public bool RemoveConnectionAtIndex(int index)
        {
            if (_connections == null || _connections.Length == 0 || index < 0 || index >= _connections.Length) return false;

            var newArray = new TaskConnection[_connections.Length - 1];

            int offset = 0;
            for (int i = 0; i < _connections.Length; i++)
            {
                if (i == index)
                {
                    offset = 1;
                    continue;
                }

                newArray[i - offset] = _connections[i];
            }

            SetConnections(newArray);
            return true;
        }

        public bool AddConnection(int outPin, int inNode)
        {
            bool hasConnections = _connections != null && _connections.Length > 0;
            if (hasConnections && _connections.Any(c => c.OutPin == outPin && c.InNode == inNode)) return false;

            var currentConnections = _connections == null ? new TaskConnection[0] : _connections;
            var connection = new TaskConnection(outPin, inNode);
            SetConnections(currentConnections.Concat(new TaskConnection[] { connection }).ToArray());
            return true;
        }

        public void ReorderConnections(System.Func<TaskConnection, float> orderBy)
        {
            System.Array.Sort(_connections, (c, next) => orderBy(c) > orderBy(next) ? 1 : -1);
        }

        [RuntimeInstanced]
        internal void InitialzeConnections(NodeData[] treeData)
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
                TaskImplementation.Initialize();
            }
        }
    }

    [System.Serializable]
    internal class TreeNodeData
    {
        [SerializeField]
        private NodeData[] _data;

        public NodeData[] Data { get => _data; internal set => _data = value; }

        public TreeNodeData() 
        {
            Data = new NodeData[0];
        }

        public TreeNodeData(NodeData[] data)
        {
            Data = data;
        }

        public void AddData(params NodeData[] data)
        {
            _data = _data.Concat(data).ToArray();
        }

        public void RemoveData(params NodeData[] data)
        {
            _data = _data.Where(d => !data.Contains(d)).ToArray();
        }
    }
}
