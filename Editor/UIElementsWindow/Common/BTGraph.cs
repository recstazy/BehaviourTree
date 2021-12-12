using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTGraph : GraphView, IDisposable
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
            graphViewChanged += GraphChanged;
            this.AddManipulator(new ContentDragger());
            var selectionDragger = new SelectionDragger();
            this.AddManipulator(selectionDragger);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(minScale, maxScale);
        }

        public void Dispose()
        {
            graphViewChanged -= GraphChanged;
            _nodes = null;
        }

        public void CreateNodes(BehaviourTree tree)
        {
            _nodes = new List<BTNode>();
            var nodeData = tree.NodeData;

            foreach (var n in nodeData.Data)
            {
                GenerateNode(n);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", ContextCreateDataAndNode, DropdownMenuAction.Status.Normal);
            base.BuildContextualMenu(evt);
        }

        private BTNode GenerateNode(NodeData data)
        {
            var node = new BTNode(data);
            _nodes.Add(node);
            AddElement(node);
            return node;
        }

        private void ContextCreateDataAndNode(DropdownMenuAction args)
        {
            var data = new NodeData(GUID.Generate().ToString(), null, null);
            data.Position = args.eventInfo.mousePosition;
            Tree.NodeData.AddData(data);
            BTWindow.SetDirty("Add Node");
            GenerateNode(data);
        }

        private GraphViewChange GraphChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                var data = ToNodes(change.elementsToRemove)
                    .Select(n => n.Data)
                    .ToArray();

                Tree.NodeData.RemoveData(data);
                BTWindow.SetDirty("Delete Nodes");
            }
            else if (change.movedElements != null)
            {
                var nodes = ToNodes(change.movedElements).ToArray();

                foreach (var n in nodes)
                {
                    n.Data.Position = n.GetPosition().position;
                }

                BTWindow.SetDirty("Move Nodes");
            }

            return change;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
        }

        private IEnumerable<BTNode> ToNodes(IEnumerable<GraphElement> elements)
        {
            return elements.Where(e => e is BTNode).Select(e => (e as BTNode));
        }
    }
}
