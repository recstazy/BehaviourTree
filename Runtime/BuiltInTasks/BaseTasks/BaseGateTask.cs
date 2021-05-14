using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base task to derive if you want to implement a gate
    /// </summary>
    [ExcludeFromTaskSelector]
    public class BaseGateTask : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private bool invert = false;

        #endregion

        #region Properties

        #endregion

        /// <summary> Override to implement your gate open condition check </summary>
        protected virtual bool CheckGateCondition()
        {
            return false;
        }

        /// <summary> What condition we're checking? </summary>
        protected virtual string GetGateDescription()
        {
            return "";
        }

        public override string GetDescription()
        {
            return $"{(invert ? "Fail" : "Succeed")} if {GetGateDescription()}";
        }

        protected override IEnumerator TaskRoutine()
        {
            yield return null;
            var condition = CheckGateCondition();
            Succeed = invert ? !condition : condition;
        }

        internal bool CheckGateConditionInEditor()
        {
            if (Blackboard is null) return false;
            return CheckGateCondition();
        }
    }
}
