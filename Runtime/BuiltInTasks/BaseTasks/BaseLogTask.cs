using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    public abstract class BaseLogTask : BehaviourTask
    {
        public enum LogType { Log, Warning, Error }
        #region Fields

        [SerializeField]
        protected LogType _logType;

        #endregion

        #region Properties

        public LogType LogStringType => _logType;

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            switch (_logType)
            {
                case LogType.Log:
                    Debug.Log(GetLogString());
                    break;
                case LogType.Warning:
                    Debug.LogWarning(GetLogString());
                    break;
                case LogType.Error:
                    Debug.LogError(GetLogString());
                    break;
            }

            yield break;
        }

        protected abstract string GetLogString();
    }
}
