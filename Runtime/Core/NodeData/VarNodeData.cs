using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    internal class VarNodeData : NodeData, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _variableName;

        [SerializeField]
        private string _variableTypeName;

        public string VariableName { get => _variableName; }
        public Type VariableType { get; private set; }
        public string VariableTypeName { get => _variableTypeName; }

        public VarNodeData(int index, string variableName, Type varType, params TaskConnection[] connections) : base(index, connections)
        {
            _variableName = variableName;
            VariableType = varType;
            _variableTypeName = VariableType.FullName;
        }

        public VarNodeData(int index, string variableName, string varTypeName, params TaskConnection[] connections) : base(index, connections)
        {
            _variableName = variableName;
            _variableTypeName = varTypeName;
            VariableType = Type.GetType(varTypeName);
        }

        public void OnBeforeSerialize()
        {
            _variableTypeName = VariableType?.FullName;
        }

        public void OnAfterDeserialize()
        {
            VariableType = Type.GetType(_variableTypeName);
        }

        [RuntimeInstanced]
        internal override void InitialzeConnections(IEnumerable<NodeData> nodeData, Blackboard blackboard)
        {
            base.InitialzeConnections(nodeData, blackboard);

            foreach (var c in Connections)
            {
                var nodeWithIndex = nodeData.FirstOrDefault(n => n.Index == c.InNode);

                if (nodeWithIndex != null)
                {
                    if (blackboard.GetterValues.TryGetValue(_variableName, out var accessor))
                    {
                        var input = nodeWithIndex.GetGetter(c.InName);
                        input?.InitializeMethod(accessor.GenericDelegate);
                    }
                }
            }
        }
    }
}
