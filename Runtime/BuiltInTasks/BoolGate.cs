using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Succeed if blackboard boolean is true, else fail
    /// </summary>
    [TaskOut]
    [TaskMenu("Gate/Bool Gate")]
    public class BoolGate : BaseGateTask
    {
        #region Fields

        [SerializeField]
        private InputValue<bool> _bool;

        #endregion

        #region Properties

        #endregion

        protected override bool CheckGateCondition()
        {
            return _bool.Value;
        }
    }
}
