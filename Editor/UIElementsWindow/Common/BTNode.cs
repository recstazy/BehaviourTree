using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNode : Node
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
            title = GetName();
        }

        protected string GetName()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }
    }
}
