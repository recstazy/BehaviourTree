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
        private bool _isInitialized;
        private MousePositionProviderManipulator _mousePositionProvider;

        #endregion

        #region Properties

        public BehaviourTree Tree { get; private set; }

        protected override bool canCopySelection => true;
        protected override bool canPaste => true;
        private Vector2 MousePos => _mousePositionProvider == null ? Vector2.zero : _mousePositionProvider.MousePos;

        #endregion

        public void Initialize(BehaviourTree tree)
        {
            Tree = tree;
            CreateNodes(tree);
            CreateEdges();
            graphViewChanged += GraphChanged;
            this.AddManipulator(new ContentDragger());
            var selectionDragger = new SelectionDragger();
            this.AddManipulator(selectionDragger);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new RectangleSelector());
            _mousePositionProvider = new MousePositionProviderManipulator();
            this.AddManipulator(_mousePositionProvider);
            SetupZoom(minScale, maxScale);

            serializeGraphElements += SerializeForCopy;
            unserializeAndPaste += UnserializeAndPaste;
            _isInitialized = true;
        }

        public void Dispose()
        {
            serializeGraphElements -= SerializeForCopy;
            unserializeAndPaste -= UnserializeAndPaste;
            graphViewChanged -= GraphChanged;
            _nodes = null;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", ContextCreateDataAndNode, DropdownMenuAction.Status.Normal);
            base.BuildContextualMenu(evt);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Remove all ports on same node and all ports with same direction (in/out)
            var availablePorts = ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToArray();
            var connectedPorts = startPort.connections.Select(c => startPort.direction == Direction.Input ? c.output : c.input).ToArray();
            // Remove all ports which we already are connected with to avoid multi-edge generation on both in and out ports multi capacity
            return availablePorts.Where(p => !connectedPorts.Contains(p)).ToList();
        }

        private void CreateNodes(BehaviourTree tree)
        {
            _nodes = new List<BTNode>();
            var nodeData = tree.NodeData;

            foreach (var n in nodeData.Data)
            {
                GenerateNode(n);
            }
        }

        private BTNode GenerateNode(NodeData data)
        {
            var node = new BTNode(data);
            _nodes.Add(node);
            AddElement(node);
            node.ApplyPositionFromData();
            return node;
        }

        private void CreateEdges()
        {
            foreach (var n in _nodes)
            {
                CreateEdgesForNode(n);
            }
        }

        private void CreateEdgesForNode(BTNode n)
        {
            var outputPorts = ports.ToList().Where(p => p.direction == Direction.Output).ToArray();
            if (outputPorts.Length == 0) return;

            foreach (var c in n.Data.Connections)
            {
                var outPort = outputPorts.First(p => p.node == n && (int)p.userData == c.OutPin);
                var inPort = _nodes.First(n => n.Data.Index == c.InNode).inputContainer.Q<Port>();
                var edge = outPort.ConnectTo(inPort);
                AddElement(edge);
            }
        }

        private void ContextCreateDataAndNode(DropdownMenuAction args)
        {
            var data = new NodeData(GetAvailableNodeIndex(), null, null);
            data.Position = _mousePositionProvider.MousePos;
            Tree.NodeData.AddData(data);
            BTWindow.SetDirty("Add Node");
            GenerateNode(data);
        }

        private GraphViewChange GraphChanged(GraphViewChange change)
        {
            if (!_isInitialized) return change;

            if (change.elementsToRemove != null)
            {
                // Remove entry node from change
                change.elementsToRemove = change.elementsToRemove.Where(e => !e.IsEntryNode()).ToList();
                var edges = change.elementsToRemove.Where(e => e is Edge).Select(e => (Edge)e).ToArray();

                if (edges.Length > 0)
                {
                    var nodesToUpdate = edges.Select(e => e.output.node as BTNode).ToList();

                    foreach (var e in edges)
                    {
                        RemoveConnectionByEdge(e);
                    }

                    BTWindow.SetDirty("Delete Connectiions");

                    foreach (var n in nodesToUpdate)
                    {
                        n.EdgesChanged();
                    }
                }

                var nodes = ToNodes(change.elementsToRemove).ToArray();
                var data = nodes.Select(n => n.Data).ToArray();

                if (data.Length > 0)
                {
                    Tree.NodeData.RemoveData(data);
                    BTWindow.SetDirty("Delete Nodes");

                    foreach (var n in nodes)
                    {
                        _nodes.Remove(n);
                    }

                    BTNode.AnyNodeDeleted();
                }
            }
            else if (change.movedElements != null)
            {
                var nodes = ToNodes(change.movedElements).ToArray();

                foreach (var n in nodes)
                {
                    n.Data.Position = n.GetWorldPosition();
                }

                BTWindow.SetDirty("Move Nodes");
            }
            else if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                var nodesToUpdate = change.edgesToCreate.Select(e => e.output.node as BTNode).ToList();

                foreach (var e in change.edgesToCreate)
                {
                    CreateConnectionByEdge(e);
                }

                BTWindow.SetDirty("Connect Nodes");

                foreach (var n in nodesToUpdate)
                {
                    n.EdgesChanged();
                }
            }

            return change;
        }

        private void RemoveConnectionByEdge(Edge edge)
        {
            var outputNode = edge.output.node as BTNode;
            var inputNode = edge.input.node as BTNode;
            
            if (outputNode.Data.TryFindIndexOfConnection((int)edge.output.userData, inputNode.Data.Index, out int connectionIndex))
            {
                outputNode.Data.RemoveConnectionAtIndex(connectionIndex);
            }
        }

        private void CreateConnectionByEdge(Edge edge)
        {
            var outputNode = edge.output.node as BTNode;
            var inputNode = edge.input.node as BTNode;
            // Port user data is output index
            outputNode.Data.AddConnection((int)edge.output.userData, inputNode.Data.Index);
        }

        private IEnumerable<BTNode> ToNodes(IEnumerable<GraphElement> elements)
        {
            return elements.Where(e => e is BTNode).Select(e => (e as BTNode));
        }

        private int GetAvailableNodeIndex()
        {
            if (_nodes.Count > 0) return _nodes.Max(n => n.Data.Index) + 1;
            else return 0;
        }

        private string SerializeForCopy(IEnumerable<GraphElement> elements)
        {
            string serializedString = CopyPasteSerializer.Serialize(elements);
            return serializedString;
        }

        private void UnserializeAndPaste(string operationName, string dataString)
        {
            var newNodeData = CopyPasteSerializer.Deserialize(dataString, GetAvailableNodeIndex(), MousePos);
            Tree.NodeData.AddData(newNodeData);
            var newNodes = new List<BTNode>();

            foreach (var data in newNodeData)
            {
                var newNode = GenerateNode(data);
                newNodes.Add(newNode);
            }

            foreach (var node in newNodes)
            {
                CreateEdgesForNode(node);
            }
        }
    }
}
