using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Executes random connection
    /// </summary>
    [NoInspector]
    [TaskMenu("Tasks/Run Random")]
    public class RunRandom : MultioutTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return "Run random out";
        }

        protected override int GetCurrentOutIndex()
        {
            return Random.Range(0, Connections.Count);
        }
    }
}
