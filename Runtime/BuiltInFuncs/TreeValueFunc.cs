using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    internal struct AnyValueType { }

    public class TreeValueFunc : VariableFunc
    {
        #region Fields

        [SerializeReference]
        private object _value;

        #endregion

        #region Properties

        public object Value { get => _value; }

        #endregion

        public TreeValueFunc() : base(typeof(AnyValueType)) { }

        public TreeValueFunc(object value, Type valueType) : base(valueType)
        {
            _value = value;
        }

        public override FuncOut[] GetOuts()
        {
            return new FuncOut[] { new FuncOut(VariableType, string.Empty) };
        }

        public override Delegate GetValueGetter(FuncOut funcOut)
        {
            return null;
        }
    }
}
