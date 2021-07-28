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
        private BlackboardName name;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Clear {name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.SetValue(name, null);
            yield return null;
        }
    }
}
