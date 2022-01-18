using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut]
    [TaskMenu("Value/Clear Value")]
    public class ClearValue : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private BlackboardSetter _name;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.ClearValue(_name);
            yield return null;
        }
    }
}
