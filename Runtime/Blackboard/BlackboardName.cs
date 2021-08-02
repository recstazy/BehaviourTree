using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Use this to show blackboard name selector in editor
    /// </summary>
    [Serializable]
    public class BlackboardName
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
        public static implicit operator BlackboardName(string value) => new BlackboardName() { _name = value };
    }
}
