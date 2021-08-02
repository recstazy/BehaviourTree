using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    public class BlackboardValueMapping
    {
        #region Fields

        [SerializeField]
        private BlackboardName _name;

        [SerializeReference]
        private ITypedValue _value;

        #endregion

        #region Properties

        public BlackboardName BlackboardName { get => _name; }
        public ITypedValue Value { get => _value; }

        #endregion

        public BlackboardValueMapping() { }
        
        public BlackboardValueMapping(BlackboardName name, ITypedValue value)
        {
            _name = name;
            _value = value;
        }
    }
}
