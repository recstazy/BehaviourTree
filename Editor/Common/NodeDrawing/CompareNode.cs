using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomNodeDrawer(typeof(CompareValue))]
    internal class CompareNode : BehaviourTreeNode
    {
        #region Fields

        private static readonly Color backColor = new Color(0.1f, 0.175f, 0.175f, 0.5f);

        #endregion

        #region Properties

        protected override Color BackColor => backColor;

        #endregion

        public CompareNode(NodeData data) : base(data) { }
    }
}
