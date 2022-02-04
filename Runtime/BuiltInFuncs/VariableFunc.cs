using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public abstract class VariableFunc : BehaviourFunc, ISerializationCallbackReceiver
    {
        #region Fields

        [SerializeField]
        private string _variableTypeName;

        #endregion

        #region Properties

        public Type VariableType { get; private set; }
        public string VariableTypeName { get => _variableTypeName; }

        #endregion

        public VariableFunc(Type valueType)
        {
            VariableType = valueType;
            _variableTypeName = JsonHelper.GetTypeString(valueType);
        }

        public VariableFunc(string typeName)
        {
            _variableTypeName = typeName;
            VariableType = JsonHelper.StringToType(_variableTypeName);
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
