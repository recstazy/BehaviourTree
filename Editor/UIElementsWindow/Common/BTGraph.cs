using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTGraph : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BTGraph> { }

        #region Fields

        private List<BTNode> _nodes;

        #endregion

        #region Properties

        public BehaviourTree Tree { get; private set; }

        #endregion

        public void Initialize(BehaviourTree tree)
        {
            Tree = tree;
            CreateNodes(tree);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(minScale, maxScale);
        }

        public void CreateNodes(BehaviourTree tree)
        {
            _nodes = new List<BTNode>();
            var nodeData = tree.NodeData;

            foreach (var n in nodeData.Data)
            {
                var node = new BTNode(n);
                _nodes.Add(node);
                AddElement(node);
            }
        }
    }
}
