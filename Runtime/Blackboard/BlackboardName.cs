using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base type of variable name defined in blackboard. 
    /// Use BlackboardGetter and BlackboardSetter to define property name which has "get" or "set" accessor
    /// </summary>
    public abstract class BlackboardName
    {
        #region Fields

        [SerializeField]
        private string _name;

        #endregion

        #region Properties

        public string Name => _name;

        #endregion

        public override string ToString()
        {
            return _name;
        }

        public static implicit operator string(BlackboardName value) => Equals(value, null) || string.IsNullOrEmpty(value._name) ? string.Empty : value._name;
    }
}
