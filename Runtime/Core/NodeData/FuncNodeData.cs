using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    internal class FuncNodeData : NodeData
    {
        [SerializeReference]
        private BehaviourFunc _funcImplementation;

        public BehaviourFunc FuncImplementation { get => _funcImplementation; set => _funcImplementation = value; }
        internal override object Implementation { get => _funcImplementation; }

        public FuncNodeData(int index, BehaviourFunc func, params TaskConnection[] connections) : base(index, connections)
        {
            _funcImplementation = func;
        }

        [RuntimeInstanced]
        internal override void InitialzeConnections(IEnumerable<NodeData> nodeData)
        {
            base.InitialzeConnections(nodeData);
            var outputs = _funcImplementation?.GetOuts();
            if (outputs == null || outputs.Length == 0) return;

            foreach (var c in Connections)
            {
                var nodeWithIndex = nodeData.FirstOrDefault(n => n.Index == c.InNode);

                if (nodeWithIndex != null)
                {
                    var del = _funcImplementation.GetValueGetter(outputs[c.OutPin]);
                    var input = nodeWithIndex.GetGetter(c.InName);
                    input?.InitializeMethod(del);
                }
            }
        }
    }
}
