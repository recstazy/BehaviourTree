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

        [SerializeField]
        private SerializedValue _value;

        #endregion

        #region Properties

        public SerializedValue Value { get => _value; set => _value = value; }

        #endregion

        public TreeValueFunc() : base(typeof(AnyValueType)) { }

        public TreeValueFunc(object value, Type type) : base(type)
        {
            _value = new SerializedValue(value);
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
