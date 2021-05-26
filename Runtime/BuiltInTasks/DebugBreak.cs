using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [NoInspector]
    public class DebugBreak : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return "Pause Playmode";
        }

        protected override IEnumerator TaskRoutine()
        {
            Succeed = true;
            Debug.Break();
            yield return null;
        }
    }
}
