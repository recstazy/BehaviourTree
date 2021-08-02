using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard value with editable value name
    /// </summary>
    [System.Serializable]
    public class TypedValue
    {
        #region Fields

        [SerializeField]
        private string _name;

        [SerializeReference]
        private ITypedValue _value;

        #endregion

        #region Properties

        public string Name => _name;
        public ITypedValue Value => _value;

        #endregion

        public TypedValue(string name, ITypedValue value)
        {
            _name = name;
            _value = value;
        }

        internal void ChangeName(string newName)
        {
            _name = newName;
        }
    }
}
