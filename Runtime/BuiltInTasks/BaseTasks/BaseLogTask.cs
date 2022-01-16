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

        private static readonly Color s_logBackColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private static readonly Color s_warningBackColor = new Color(0.5f, 0.4f, 0.1f, 0.5f);
        private static readonly Color s_errorBackColor = new Color(0.7f, 0.2f, 0.15f, 0.5f);

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

        private Color GetDrawColor()
        {
            switch (_logType)
            {
                case LogType.Log:
                    return s_logBackColor;
                case LogType.Warning:
                    return s_warningBackColor;
                case LogType.Error:
                    return s_errorBackColor;
                default:
                    return s_logBackColor;
            }
        }
    }
}
