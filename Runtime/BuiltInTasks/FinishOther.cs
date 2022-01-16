using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskMenu("Tasks/Finish Other")]
    public class FinishOther : MultioutTask
    {
        #region Fields

        [SerializeField]
        private bool _succeedAll = true;

        [SerializeField]
        private bool _succeedSelf = true;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            foreach (var c in Connections)
            {
                c?.ForceFinishTask(_succeedAll);
            }

            Succeed = _succeedSelf;
            yield return null;
        }

        protected override int GetCurrentOutIndex()
        {
            return NoOut;
        }
    }
}
