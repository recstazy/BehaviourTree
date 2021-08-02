using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Value/Clear Value")]
    public class ClearValue : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private BlackboardName _name;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Clear {_name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.SetValue(_name, null);
            yield return null;
        }
    }
}
