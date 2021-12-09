using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNode : Draggable
    {
        #region Fields

        #endregion

        #region Properties
	
        public NodeData Data { get; private set; }

        #endregion

        public BTNode() { }

        public BTNode(NodeData data) : base()
        {
            transform.position = data.Position;
            AddToClassList("bt-node");
        }
    }
}
