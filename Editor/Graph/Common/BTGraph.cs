using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTGraph : GraphView, IDisposable, IPlaymodeDependent
    {
        public new class UxmlFactory : UxmlFactory<BTGraph> { }
        public event Action OnStructureChanged;

        #region Fields

        private List<BTNode> _nodes;
        private bool _isInitialized;
        private BTMousePosProvider _mousePosProvider;
        private bool _isPlaymode;

        #endregion

        #region Properties

        public BehaviourTree Tree { get; private set; }
        public Vector3 CurrentPosition => viewTransform.position;
        public float CurrentZoom => viewTransform.scale.x;

        internal ReadOnlyCollection<BTNode> BtNodes => _nodes.AsReadOnly();

        protected override bool canCutSelection => !_isPlaymode;
        protected override bool canDeleteSelection => !_isPlaymode;
        protected override bool canDuplicateSelection => !_isPlaymode;
        protected override bool canCopySelection => !_isPlaymode;
        protected override bool canPaste => !_isPlaymode;
        private Vector2 MousePosition => _mousePosProvider == null ? Vector2.zero : _mousePosProvider.MousePosition;

        #endregion

        public void Initialize()
        {
            graphViewChanged += GraphChanged;

            var contentDragger = new ContentDragger();
            this.AddManipulator(contentDragger);
            var selectionDragger = new SelectionDragger();
            this.AddManipulator(selectionDragger);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new RectangleSelector());
            _mousePosProvider = new BTMousePosProvider();
            this.AddManipulator(_mousePosProvider);
            SetupZoom(minScale, maxScale);

            serializeGraphElements += SerializeForCopy;
            unserializeAndPaste += UnserializeAndPaste;
            _isInitialized = true;
        }

        public void SetTree(BehaviourTree tree)
        {
            ClearAll();

            Tree = tree;
            CreateNodes(tree);
            CreateEdges();
            BTNode.ValidateOuts();

            Vector3 zoom = new Vector3(tree.Zoom, tree.Zoom, 1);
            UpdateViewTransform(tree.GraphPosition, zoom);
        }

        public void ClearAll()
        {
            edges.ForEach(e => RemoveElement(e));

            if (_nodes != null)
            {
                foreach (var n in _nodes)
                {
                    n.OnReconnect -= ReconnectNode;
                    n.Dispose();
                    RemoveElement(n);
                }
            }

            _nodes = null;
            Tree = null;
        }

        public void Dispose()
        {
            ClearAll();

            serializeGraphElements -= SerializeForCopy;
            unserializeAndPaste -= UnserializeAndPaste;
            graphViewChanged -= GraphChanged;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Remove all ports on same node and all ports with same direction (in/out)
            // Get only type-compatable ports
            var availablePorts = ports.ToList()
                .Where(p => p.direction != startPort.direction && p.node != startPort.node)
                .ToArray();

            // Get only type-compatable ports
            // I know I can better but...
            if (startPort.portType == typeof(AnyValueType))
            {
                availablePorts = availablePorts
                    .Where(p => p.portType != typeof(ExecutionPin) && !typeof(UnityEngine.Object).IsAssignableFrom(p.portType))
                    .ToArray();
            }
            else
            {
                availablePorts = availablePorts
                    .Where(p => p.portType == startPort.portType)
                    .ToArray();
            }

            var connectedPorts = startPort.connections.Select(c => startPort.direction == Direction.Input ? c.output : c.input).ToArray();
            // Remove all ports which we already are connected with to avoid multi-edge generation on both in and out ports multi capacity
            return availablePorts.Where(p => !connectedPorts.Contains(p)).ToList();
        }

        public void PlaymodeChanged(bool isPlaymode)
        {
            _isPlaymode = isPlaymode;
            edges.ForEach(e => e.SetEnabled(!isPlaymode));
            nodes.ForEach(e => e.SetEnabled(!isPlaymode));
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!_isPlaymode)
            {
                evt.menu.AppendAction("Create Task", ContextCreateTaskAndNode, DropdownMenuAction.Status.Normal);
                evt.menu.AppendAction("Create Value", ContextualCreateTreeValueAndNode, DropdownMenuAction.Status.Normal);

                if (Tree.Blackboard != null)
                {
                    var getterNames = Tree.Blackboard.GetNames(BlackboardProperty.Getter);

                    foreach (var n in getterNames)
                    {
                        evt.menu.AppendAction($"Get/{n}", ContextCreateBbValueAndNode, (evt) => DropdownMenuAction.Status.Normal, n);
                    }
                }
                else evt.menu.AppendAction("Get", null, DropdownMenuAction.Status.Disabled);

                evt.menu.AppendSeparator();
            }

            base.BuildContextualMenu(evt);
        }

        private void ContextCreateTaskAndNode(DropdownMenuAction args)
        {
            var data = new TaskNodeData(GetAvailableNodeIndex(), null, null);
            ContextCreateNode(data);
        }

        private void ContextCreateBbValueAndNode(DropdownMenuAction args)
        {
            var variableName = (string)args.userData;
            var accessor = Tree.Blackboard.GetterValues[variableName];
            var func = new BbValueFunc(accessor.PropertyType, variableName);
            var data = new FuncNodeData(GetAvailableNodeIndex(), func, null);
            ContextCreateNode(data);
        }

        private void ContextualCreateTreeValueAndNode(DropdownMenuAction args)
        {
            var impl = new TreeValueFunc();
            var data = new FuncNodeData(GetAvailableNodeIndex(), impl, null);
            ContextCreateNode(data);
        }

        private void ContextCreateNode(NodeData data)
        {
            data.Position = _mousePosProvider.MousePosition;
            Tree.NodeData.AddData(data);
            BTWindow.SetDirty("Add Node");
            GenerateNode(data);
            OnStructureChanged?.Invoke();
        }

        private void CreateNodes(BehaviourTree tree)
        {
            _nodes = new List<BTNode>();
            var nodeData = tree.NodeData;

            foreach (var n in nodeData)
            {
                GenerateNode(n);
            }
        }

        private BTNode GenerateNode(NodeData data)
        {
            var node = data.CreateGraphNode();
            node.Owner = this;
            _nodes.Add(node);
            node.OnReconnect += ReconnectNode;
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
            var nodeOutputs = n.outputContainer.Query<Port>().Build().ToList();

            foreach (var c in n.Data.Connections)
            {
                var outPort = nodeOutputs.First(p => p.GetOutDescription().Index == c.OutPin);
                var inNode = _nodes.First(node => node.Data.Index == c.InNode);
                var inNodeInputs = inNode.inputContainer.Query<Port>().Build().ToList();
                var inPort = inNodeInputs.First(p => p.GetInputDescription().IdName == c.InName);
                var edge = outPort.ConnectTo(inPort);

                AddElement(edge);
            }
        }

        private void ReconnectNode(BTNode node)
        {
            var ports = node.outputContainer.Query<Port>().Build().ToList();

            foreach (var p in ports)
            {
                var portEdges = p.connections.ToArray();

                foreach (var edge in portEdges)
                {
                    edge.input.Disconnect(edge);
                    edge.output.Disconnect(edge);
                    RemoveElement(edge);
                }
            }

            node.UpdateAllPorts();
            CreateEdgesForNode(node);
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
                    var nodesToUpdate = RemoveConnectionsByEdges(edges);
                    BTWindow.SetDirty("Delete Connections");

                    foreach (var n in nodesToUpdate)
                    {
                        n.EdgesChangedExternally();
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
                        n.OnReconnect -= ReconnectNode;
                        n.WasDeleted();
                    }

                    BTNode.AnyNodeDeleted();
                }

                OnStructureChanged?.Invoke();
            }
            else if (change.movedElements != null)
            {
                var nodes = ToNodes(change.movedElements).ToArray();

                foreach (var n in nodes)
                {
                    n.Data.Position = n.GetWorldPosition();
                }

                BTWindow.SetDirty($"Move Nodes");
            }
            else if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                var nodesToUpdate = new HashSet<BTNode>(change.edgesToCreate.Select(e => e.output.node as BTNode).ToArray());

                foreach (var e in change.edgesToCreate)
                {
                    CreateConnectionByEdge(e);
                }

                foreach (var n in nodesToUpdate)
                {
                    n.EdgesChangedExternally();
                }

                BTWindow.SetDirty("Connect Nodes");
                OnStructureChanged?.Invoke();
            }

            return change;
        }

        private bool TryGetConnectionIndex(Edge edge, out int index)
        {
            var outputNode = edge.output.node as BTNode;
            var inputNode = edge.input.node as BTNode;
            var outDesc = edge.output.GetOutDescription();
            var inputDesc = edge.input.GetInputDescription();
            return outputNode.Data.TryFindIndexOfConnection(outDesc.Index, inputNode.Data.Index, inputDesc.IdName, out index);
        }

        // Remove connections in data and return modified nodes
        private BTNode[] RemoveConnectionsByEdges(Edge[] edges)
        {
            var grouped = edges.GroupBy(e => e.output.node).ToDictionary(group => (BTNode)group.Key, group => group.ToArray());
            List<int> validConnectionsToDelete = new List<int>();

            foreach (var group in grouped)
            {
                validConnectionsToDelete.Clear();

                foreach (var edge in group.Value)
                {
                    if (TryGetConnectionIndex(edge, out var index))
                    {
                        validConnectionsToDelete.Add(index);
                    }
                }

                group.Key.Data.RemoveConnectionsWithIndices(validConnectionsToDelete.ToArray());
            }

            return grouped.Select(pair => pair.Key).ToArray();
        }

        private void CreateConnectionByEdge(Edge edge)
        {
            var outputNode = edge.output.node as BTNode;
            var inputNode = edge.input.node as BTNode;
            var outDesc = edge.output.GetOutDescription();
            var inputDesc = edge.input.GetInputDescription();
            outputNode.Data.AddConnection(outDesc.Index, inputNode.Data.Index, inputDesc.IdName, inputDesc.PortType);
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
            var newNodeData = CopyPasteSerializer.Deserialize(dataString, GetAvailableNodeIndex(), MousePosition);
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

            OnStructureChanged?.Invoke();
            BTWindow.SetDirty("Paste Elements");
        }
    }
}
