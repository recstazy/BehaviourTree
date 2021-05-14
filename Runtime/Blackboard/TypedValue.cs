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
        private string name;

        [SerializeReference]
        private ITypedValue value;

        #endregion

        #region Properties

        public string Name => name;
        public ITypedValue Value => value;

        #endregion

        public TypedValue(string name, ITypedValue value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
