using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public class FinishOther : MultioutTask
    {
        #region Fields

        [SerializeField]
        private bool succeedAll = true;

        [SerializeField]
        private bool succeedSelf = true;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"{(succeedAll ? "Succeed" : "Fail")} all connected tasks";
        }

        protected override IEnumerator TaskRoutine()
        {
            foreach (var c in Connections)
            {
                c?.ForceFinishTask(succeedAll);
            }

            Succeed = succeedSelf;
            yield return null;
        }

        protected override int GetCurrentOutIndex()
        {
            return NoOut;
        }
    }
}
