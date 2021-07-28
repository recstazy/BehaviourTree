using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Log a string to console from BehaviourTree
    /// </summary>
    [TaskOut(0)]
    [TaskMenu("Debug/Log")]
    public class Log : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private string logString;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Log: {logString}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Debug.Log(logString);
            Succeed = true;
            yield return null;
        }
    }
}
