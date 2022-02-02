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

        [SerializeField]
        private int _index;

        [SerializeField]
        private TaskConnection[] _connections;

        #endregion

        #region Properties

        public int Index => _index;
        internal TaskConnection[] Connections { get => _connections; set => _connections = value; }
        internal Vector2 Position { get => _position; set => _position = value; }

        #endregion

        public NodeData()
        {
            Connections = new TaskConnection[0];
        }

        public NodeData(int index, params TaskConnection[] connections)
        {
            _index = index;
            SetConnections(connections);
        }

        public void SetConnections(params TaskConnection[] connections)
        {
            if (connections == null) Connections = new TaskConnection[0];
            else Connections = connections;
        }

        public bool TryFindIndexOfConnection(int outPin, int inNodeIndex, string inName, out int connectionIndex)
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                var c = _connections[i];

                if (c.OutPin == outPin && c.InNode == inNodeIndex && c.InName == inName)
                {
                    connectionIndex = i;
                    return true;
                }
            }

            connectionIndex = -1;
            return false;
        }

        public void RemoveConnectionsWithIndices(params int[] indicies)
        {
            if (_connections == null || _connections.Length == 0 || indicies == null || indicies.Length == 0) return;
            var newArray = _connections.Where((c, index) => !indicies.Contains(index)).ToArray();
            newArray = PostProcessConnectionsAfterChange(newArray);

            SetConnections(newArray);
        }

        public bool AddConnection(int outPin, int inNode, string inName, System.Type connectionType = null)
        {
            bool hasConnections = _connections != null && _connections.Length > 0;
            if (hasConnections && _connections.Any(c => c.OutPin == outPin && c.InNode == inNode && c.InName == inName)) return false;

            var currentConnections = _connections == null ? new TaskConnection[0] : _connections;
            var connection = new TaskConnection(outPin, inNode, inName, connectionType);
            var newArray = currentConnections.Concat(new TaskConnection[] { connection }).ToArray();
            newArray = PostProcessConnectionsAfterChange(newArray);

            SetConnections(newArray);
            return true;
        }

        protected virtual TaskConnection[] PostProcessConnectionsAfterChange(TaskConnection[] newArray)
        {
            return newArray;
        }

        [RuntimeInstanced]
        internal virtual void InitialzeConnections(IEnumerable<NodeData> nodeData, Blackboard blackboard) { }
    }
}
