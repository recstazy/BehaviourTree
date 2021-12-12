using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Gate/Is Set Gate")]
    public class ValueSetGate : BaseGateTask
    {
        #region Fields

        [SerializeField]
        private BlackboardGetter _value;

        #endregion

        #region Properties

        #endregion

        protected override string GetGateDescription()
        {
            return $"{_value} is set";
        }

        protected override bool CheckGateCondition()
        {
            var isSet = Blackboard.IsValueSetAndNotNull(_value);
            return isSet;
        }
    }
}
