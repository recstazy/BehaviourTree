using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
            Data = data;
            transform.position = data.Position;
            AddToClassList("bt-node");
        }

        protected override void EndDrag(MouseUpEvent evt)
        {
            base.EndDrag(evt);
            UpdateData();
            BTWindow.SetDirty("Move Node");
        }

        private void UpdateData()
        {
            Data.Position = transform.position;
        }
    }
}
