using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut]
    [NoInspector]
    [TaskMenu("Debug/Break")]
    public class DebugBreak : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            Succeed = true;
            if (Application.isEditor) Debug.Break();
            yield return null;
        }
    }
}
