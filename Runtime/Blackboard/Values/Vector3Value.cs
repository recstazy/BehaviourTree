using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard vector representation
    /// </summary>
    [System.Serializable]
    public struct Vector3Value : ITypedValue
    {
        #region Fields

        [SerializeField]
        private Vector3 vectorValue;

        #endregion

        #region Properties

        public Vector3 Value => vectorValue;
        public object MainValue => vectorValue;

        #endregion

        public CompareResult Compare(ITypedValue other)
        {
            if (other is Vector3Value vecValue)
            {
                if (vecValue.vectorValue == vectorValue)
                {
                    return CompareResult.Equal;
                }
                else return CompareResult.NotEqual;
            }

            return CompareResult.TypeError;
        }

        public static implicit operator Vector3(Vector3Value value) => value.vectorValue;
        public static implicit operator Vector3Value(Vector3 value) => new Vector3Value() { vectorValue = value };
    }
}
