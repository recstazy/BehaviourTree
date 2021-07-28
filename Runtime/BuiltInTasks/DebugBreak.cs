using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [NoInspector]
    [TaskMenu("Debug/Break")]
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
            if (Application.isEditor) Debug.Break();
            yield return null;
        }
    }
}
