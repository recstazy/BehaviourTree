using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal class VarNodeData : NodeData
    {
        [SerializeField]
        private string _variableName;

        public string VariableName { get => _variableName; }

        public VarNodeData(int index, string variableName, params TaskConnection[] connections) : base(index, connections)
        {
            _variableName = variableName;
        }
    }
}
