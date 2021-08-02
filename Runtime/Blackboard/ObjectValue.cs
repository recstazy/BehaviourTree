using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Generic base for reference type blackboard values.
    /// MUST implement empty constructor!
    /// </summary>
    /// <typeparam name="T">Reference type</typeparam>
    public class ObjectValue<T> : ITypedValue where T : class
    {
        #region Fields

        [SerializeField]
        private T _value;

        #endregion

        #region Properties

        /// <summary> Actual contained value </summary>
        public T Value => _value;

        public object MainValue => Value;

        #endregion

        /// <summary> Empty constructor needed for proper blackboard work </summary>
        public ObjectValue() { }

        /// <summary>
        /// Set contained value on construct
        /// </summary>
        public ObjectValue(T value)
        {
            _value = value;
        }

        public CompareResult Compare(ITypedValue other)
        {
            if (other is null)
            {
                return Value == null ? CompareResult.Equal : CompareResult.NotEqual;
            }
            else return CompareTo(other);
        }

        /// <summary> Override compare logic of custom reference blackboard type </summary>
        /// <param name="other">Calue witch we're comparing with</param>
        protected virtual CompareResult CompareTo(ITypedValue other)
        {
            if (other is ObjectValue<T> otherGeneric)
            {
                return otherGeneric._value.Equals(Value) ? CompareResult.Equal : CompareResult.NotEqual;
            }

            return CompareResult.TypeError;
        }

        public static implicit operator T(ObjectValue<T> value) => value.Value;
        public static implicit operator ObjectValue<T>(T value) => new ObjectValue<T>(value);
    }
}
