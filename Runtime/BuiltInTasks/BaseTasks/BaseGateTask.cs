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
        private bool _invert = false;

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
            return $"{(_invert ? "Fail" : "Succeed")} if {GetGateDescription()}";
        }

        protected override IEnumerator TaskRoutine()
        {
            yield return null;
            Succeed = GetFinalCondition();
        }

        internal bool CheckGateConditionInEditor()
        {
            if (Blackboard == null) return false;
            return GetFinalCondition();
        }

        private bool GetFinalCondition()
        {
            var condition = CheckGateCondition();
            return _invert ? !condition : condition;
        }
    }
}
