using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomNodeDrawer(typeof(BaseGateTask))]
    [CustomNodeDrawer(typeof(BoolGate))]
    [CustomNodeDrawer(typeof(ValueSetGate))]
    internal class GateNode : BehaviourTreeNode
    {
        #region Fields

        private static readonly Color s_backColorOpen = new Color(0.1f, 0.175f, 0.1f, 0.5f);
        private static readonly Color s_backColorClosed = new Color(0.175f, 0.1f, 0.1f, 0.5f);

        #endregion

        #region Properties

        protected override Color BackColor => GetBackColor();

        #endregion

        public GateNode(NodeData data) : base(data) { }

        private Color GetBackColor()
        {
            if (BTModeManager.IsPlaymode)
            {
                if (Data.TaskImplementation is BaseGateTask gateTask)
                {
                    bool result = gateTask.CheckGateConditionInEditor();
                    return result ? s_backColorOpen : s_backColorClosed;
                }
            }
            
            return s_backColorOpen;
        }
    }
}
