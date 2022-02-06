using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    public abstract class BehaviourFunc : INodeImplementation
    {
        public class FuncOut
        {
            public Type ValueType { get; private set; }
            public string ValueName { get; private set; }

            public FuncOut(Type valueType, string valueName)
            {
                ValueType = valueType;
                ValueName = valueName;
            }
        }

        #region Fields

        #endregion

        #region Properties

        [RuntimeInstanced]
        protected Blackboard Blackboard { get; private set; }

        #endregion

        public abstract FuncOut[] GetOuts();
        public abstract Delegate GetValueGetter(FuncOut funcOut);

        [RuntimeInstanced]
        internal void SetBlackboard(Blackboard blackboard)
        {
            Blackboard = blackboard;
        }
    }
}
