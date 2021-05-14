using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomNodeDrawer(typeof(SetValue))]
    internal class SetNode : BehaviourTreeNode
    {
        #region Fields

        private static readonly Color backColor = new Color(0.25f, 0.15f, 0.1f, 0.5f);

        #endregion

        #region Properties

        protected override Color BackColor => backColor;

        #endregion

        public SetNode(NodeData data) : base(data) { }
    }
}
