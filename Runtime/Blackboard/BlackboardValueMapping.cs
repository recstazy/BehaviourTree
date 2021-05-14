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
        private BlackboardName name;

        [SerializeReference]
        private ITypedValue value;

        #endregion

        #region Properties

        public BlackboardName BlackboardName { get => name; }
        public ITypedValue Value { get => value; }

        #endregion

        public BlackboardValueMapping() { }
        
        public BlackboardValueMapping(BlackboardName name, ITypedValue value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
