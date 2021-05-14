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
        private bool value;

        #endregion

        #region Properties

        public object MainValue => Value;
        public bool Value => value;
        public ValueType Type => ValueType.Bool;

        #endregion

        public CompareResult Compare(ITypedValue other)
        {
            if (other is BoolValue otherBool)
            {
                return otherBool.value == value ? CompareResult.Equal : CompareResult.NotEqual;
            }

            return CompareResult.TypeError;
        }

        public static implicit operator bool(BoolValue value) => value.value;
    }
}
