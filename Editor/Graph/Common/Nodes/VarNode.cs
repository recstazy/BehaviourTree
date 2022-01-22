using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            var outs = data.GetOuts();

            foreach (var o in outs)
            {
                CreateOutputPort(o);
            }
        }
    }
}
