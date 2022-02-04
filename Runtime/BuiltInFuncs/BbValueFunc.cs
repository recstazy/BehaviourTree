using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    public class BbValueFunc : VariableFunc
    {
        #region Fields

        [SerializeField]
        private string _variableName;

        #endregion

        #region Properties

        public string VariableName { get => _variableName; }
        
        #endregion

        public BbValueFunc(Type valueType, string valueName) : base(valueType)
        {
            _variableName = valueName;
        }

        public BbValueFunc(string typeName, string valueName) : base(typeName)
        {
            _variableName = valueName;
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

        
    }
}
