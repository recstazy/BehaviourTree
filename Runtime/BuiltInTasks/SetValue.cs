using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Set value to blackboard from behaviour tree and succeed
    /// </summary>
    [TaskOut(0)]
    [TaskMenu("Value/Set Value")]
    public class SetValue : BehaviourTask
    {
        [SerializeField]
        private BlackboardValueMapping _newValue;

        public override string GetDescription()
        {
            return $"Set {_newValue.BlackboardName}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.SetValue(_newValue.BlackboardName, _newValue.Value);
            return base.TaskRoutine();
        }
    }
}
