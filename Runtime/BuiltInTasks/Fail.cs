using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Fail the flow anyways
    /// </summary>
    [NoInspector]
    public class Fail : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return "Always fails";
        }

        protected override IEnumerator TaskRoutine()
        {
            yield return null;
            Succeed = false;
        }
    }
}
