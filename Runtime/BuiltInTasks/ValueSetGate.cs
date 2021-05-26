using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    public class ValueSetGate : BaseGateTask
    {
        #region Fields

        [SerializeField]
        private BlackboardName valueName;

        #endregion

        #region Properties

        #endregion

        protected override string GetGateDescription()
        {
            return $"{valueName} is set";
        }

        protected override bool CheckGateCondition()
        {
            return Blackboard.IsValueSet(valueName);
        }
    }
}
