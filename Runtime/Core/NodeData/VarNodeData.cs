using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
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
    }
}
