using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard float representation
    /// </summary>
    [System.Serializable]
    public struct FloatValue : ITypedValue
    {
        #region Fields

        [SerializeField]
        private float _value;

        #endregion

        #region Properties

        public object MainValue => Value;
        public float Value => _value;

        #endregion

        public FloatValue(float value)
        {
            _value = value;
        }

        public CompareResult Compare(ITypedValue other)
        {
            if (other is FloatValue otherFloat)
            {
                if (_value == otherFloat._value)
                {
                    return CompareResult.Equal;
                }
                else if (_value < otherFloat._value)
                {
                    return CompareResult.Less;
                }
                else if (_value > otherFloat._value)
                {
                    return CompareResult.More;
                }
            }

            return CompareResult.TypeError;
        }

        public static implicit operator float(FloatValue value) => value.Value;
        public static implicit operator FloatValue(float value) => new FloatValue(value);
    }
}
