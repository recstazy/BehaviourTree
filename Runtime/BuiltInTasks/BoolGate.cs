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
        private BlackboardGetter _name;

        #endregion

        #region Properties

        #endregion

        protected override string GetGateDescription()
        {
            return $"{_name} is true";
        }

        protected override bool CheckGateCondition()
        {
            if (Blackboard.TryGetValue(_name, out bool bbValue))
            {
                return bbValue;
            }
            else return false;
        }
    }
}
