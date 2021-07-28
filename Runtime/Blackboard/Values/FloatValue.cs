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
        private float value;

        #endregion

        #region Properties

        public object MainValue => Value;
        public float Value => value;

        #endregion

        public FloatValue(float value)
        {
            this.value = value;
        }

        public CompareResult Compare(ITypedValue other)
        {
            if (other is FloatValue otherFloat)
            {
                if (value == otherFloat.value)
                {
                    return CompareResult.Equal;
                }
                else if (value < otherFloat.value)
                {
                    return CompareResult.Less;
                }
                else if (value > otherFloat.value)
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
