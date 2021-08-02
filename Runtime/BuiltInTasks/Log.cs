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
        private string _logString;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Log: {_logString}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Debug.Log(_logString);
            Succeed = true;
            yield return null;
        }
    }
}
