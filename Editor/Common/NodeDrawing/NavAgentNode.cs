using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomNodeDrawer(typeof(MoveNavAgent))]
    [CustomNodeDrawer(typeof(StopNavAgent))]
    internal class NavAgentNode : BehaviourTreeNode
    {
        #region Fields

        private static readonly Color s_backColor = new Color(0f, 0.255f, 0.3f, 0.5f);

        #endregion

        #region Properties

        protected override Color BackColor => s_backColor;

        #endregion

        public NavAgentNode(NodeData data) : base(data) { }
    }
}
