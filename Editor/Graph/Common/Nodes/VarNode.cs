using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class VarNode : BTNode
    {
        #region Fields

        #endregion

        #region Properties

        public override bool IsEntry => false;

        #endregion

        public VarNode() : base() { }

        public VarNode(VarNodeData data) : base(data)
        {
            ImportLayout();
            CreateOuts();
        }

        private void ImportLayout()
        {
            titleContainer.Clear();
            titleContainer.RemoveFromHierarchy();
        }

        private void CreateOuts()
        {
            var outs = Data.GetOuts();

            foreach (var o in outs)
            {
                CreateOutputPort(o);
            }
        }
    }
}
