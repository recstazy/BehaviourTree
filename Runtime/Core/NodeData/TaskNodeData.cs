using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal class TaskNodeData : NodeData
    {
        #region Fields

        [SerializeReference]
        private BehaviourTask _taskImplementation;

        #endregion

        #region Properties

        public BehaviourTask TaskImplementation { get => _taskImplementation; internal set => _taskImplementation = value; }
        internal override object Implementation { get => _taskImplementation; }

        #endregion

        public TaskNodeData(int index, BehaviourTask taskImlpementation, params TaskConnection[] connections) : base(index, connections)
        {
            TaskImplementation = taskImlpementation;
        }

        protected override TaskConnection[] PostProcessConnectionsAfterChange(TaskConnection[] connectionsArray)
        {
            connectionsArray = base.PostProcessConnectionsAfterChange(connectionsArray);

            if (TaskImplementation != null)
            {
                connectionsArray = TaskImplementation.PostProcessConnectionsAfterChange(connectionsArray);
            }

            return connectionsArray;
        }

        [RuntimeInstanced]
        internal override void InitialzeConnections(IEnumerable<NodeData> nodeData)
        {
            base.InitialzeConnections(nodeData);

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
                    var nodeWithIndex = (TaskNodeData)nodeData.FirstOrDefault(d => d.Index == Connections[i].InNode && d is TaskNodeData);

                    if (nodeWithIndex != null)
                    {
                        connections[Connections[i].OutPin] = nodeWithIndex?.TaskImplementation;
                    }
                }

                TaskImplementation.SetRuntimeConnections(connections);
                TaskImplementation.Index = Index;
                TaskImplementation.Initialize();
            }
        }
    }
}
