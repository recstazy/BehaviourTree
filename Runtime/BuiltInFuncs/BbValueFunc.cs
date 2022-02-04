using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    public class BbValueFunc : BehaviourFunc, ISerializationCallbackReceiver
    {
        #region Fields

        [SerializeField]
        private string _variableName;

        [SerializeField]
        private string _variableTypeName;

        #endregion

        #region Properties

        public string VariableName { get => _variableName; }
        public Type VariableType { get; private set; }
        public string VariableTypeName { get => _variableTypeName; }

        #endregion

        public BbValueFunc(Type valueType, string valueName)
        {
            _variableName = valueName;
            VariableType = valueType;
            _variableTypeName = JsonHelper.GetTypeString(valueType);
        }

        public BbValueFunc(string typeName, string valueName)
        {
            _variableName = valueName;
            _variableTypeName = typeName;
            VariableType = JsonHelper.StringToType(_variableTypeName);
        }

        public override FuncOut[] GetOuts()
        {
            return new FuncOut[] { new FuncOut(VariableType, VariableName) };
        }

        public override Delegate GetValueGetter(FuncOut funcOut)
        {
            if (Blackboard.GetterValues.TryGetValue(_variableName, out var accessor))
            {
                return accessor.GenericDelegate;
            }

            return null;
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
