using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Succeed if blackboard boolean is true, else fail
    /// </summary>
    [TaskOut(0)]
    [TaskMenu("Gate/Bool Gate")]
    public class BoolGate : BaseGateTask
    {
        #region Fields

        [SerializeField]
        [ValueType(typeof(bool))]
        private BlackboardName _valueName;

        #endregion

        #region Properties

        #endregion

        protected override string GetGateDescription()
        {
            return $"{_valueName} is true";
        }

        protected override bool CheckGateCondition()
        {
            if (Blackboard.TryGetValue(_valueName, out bool bbValue))
            {
                return bbValue;
            }
            else return false;
        }
    }
}
