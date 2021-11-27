using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomNodeDrawer(typeof(BaseLogTask))]
    [CustomNodeDrawer(typeof(Log))]
    [CustomNodeDrawer(typeof(LogVariable))]
    internal class LogNode : BehaviourTreeNode
    {
        #region Fields

        private static readonly Color s_logBackColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private static readonly Color s_warningBackColor = new Color(0.5f, 0.4f, 0.1f, 0.5f);
        private static readonly Color s_errorBackColor = new Color(0.7f, 0.2f, 0.15f, 0.5f);

        #endregion

        #region Properties

        protected override Color BackColor => GetDrawColor();

        #endregion

        public LogNode(NodeData data) : base(data) { }

        private Color GetDrawColor()
        {
            var task = Data.TaskImplementation as BaseLogTask;
            if (task == null) return s_logBackColor;

            switch (task.LogStringType)
            {
                case BaseLogTask.LogType.Log:
                    return s_logBackColor;
                case BaseLogTask.LogType.Warning:
                    return s_warningBackColor;
                case BaseLogTask.LogType.Error:
                    return s_errorBackColor;
                default:
                    return s_logBackColor;
            }
        }
    }
}
