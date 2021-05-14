using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Set value to blackboard from behaviour tree and succeed
    /// </summary>
    [TaskOut(0)]
    public class SetValue : BehaviourTask
    {
        [SerializeField]
        private BlackboardName name;

        [SerializeReference]
        private ITypedValue newValue;

        public override string GetDescription()
        {
            return $"Set {name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.SetValue(name, newValue);
            return base.TaskRoutine();
        }
    }
}
