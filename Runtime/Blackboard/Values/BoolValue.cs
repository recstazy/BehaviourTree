using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard boolean representation
    /// </summary>
    [System.Serializable]
    public struct BoolValue : ITypedValue
    {
        #region Fields

        [SerializeField]
        private bool _value;

        #endregion

        #region Properties

        public object MainValue => Value;
        public bool Value => _value;
        public ValueType Type => ValueType.Bool;

        #endregion

        public BoolValue(bool value)
        {
            _value = value;
        }

        public CompareResult Compare(ITypedValue other)
        {
            if (other is BoolValue otherBool)
            {
                return otherBool._value == _value ? CompareResult.Equal : CompareResult.NotEqual;
            }

            return CompareResult.TypeError;
        }

        public static implicit operator bool(BoolValue value) => value._value;
        public static implicit operator BoolValue(bool value) => new BoolValue(value);
    }
}
