using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Fail the flow anyways
    /// </summary>
    [NoInspector]
    [TaskMenu("Tasks/Fail")]
    public class Fail : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            yield return null;
            Succeed = false;
        }
    }
}
