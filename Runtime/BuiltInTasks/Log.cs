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
    public class Log : BaseLogTask
    {
        #region Fields

        [SerializeField]
        private string _logString;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"{_logType}: {_logString}";
        }

        protected override string GetLogString()
        {
            return _logString;
        }
    }
}
