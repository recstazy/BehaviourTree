using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class NodeContainer : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<NodeContainer> { }

        #region Fields
	
        #endregion

        #region Properties
	
        public List<BTNode> Nodes { get; private set; }

        #endregion

        public NodeContainer()
        {
            if (BTWindow.SharedTree == null) return;
            Initialize();
        }

        private void Initialize()
        {
            Nodes = new List<BTNode>();
            var nodeData = BTWindow.TreeInstance.NodeData;

            foreach (var n in nodeData.Data)
            {
                var node = new BTNode(n);
                Nodes.Add(node);
                Add(node);
            }
        }
    }
}
