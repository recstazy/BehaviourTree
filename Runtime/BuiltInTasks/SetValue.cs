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
        private BlackboardName _name;

        [SerializeReference]
        private ITypedValue _newValue;

        public override string GetDescription()
        {
            return $"Set {_name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.SetValue(_name, _newValue);
            return base.TaskRoutine();
        }
    }
}
