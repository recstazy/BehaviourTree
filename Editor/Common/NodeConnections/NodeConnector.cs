using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class NodeConnector
    {
        #region Fields

        private List<BehaviourTreeNode> _nodes;
        private List<NodeDrawerIO> _ios;
        private ConnectionPin _draggedPin;
        private ConnectionPin _mouseUpPin;
        private ConnectionPin _mouseDownPin;

        private NodeDrawerIO _draggedIO;
        private NodeDrawerIO _mouseUpIO;
        private NodeDrawerIO _mouseDownIO;

        private PinConnectionDrawer performedConnectionDrawer;

        #endregion

        #region Properties

        public NodeConnectionEventArgs ConnectionPending { get; private set; }
        public NodeConnectionEventArgs RemovalPending { get; private set; }
        public bool IsRemovalPending { get; private set; }
        public bool IsConnectionPending { get; private set; }
        public bool IsPerformingConnection { get; private set; } = false;

        #endregion

        public NodeConnector(List<BehaviourTreeNode> nodes)
        {
            _nodes = nodes;
            _ios = new List<NodeDrawerIO>();

            foreach (var n in this._nodes)
            {
                CreateIO(n);
            }

            foreach (var io in _ios)
            {
                AddConnectionDrawersForIO(io);
            }
        }

        public void Dispose()
        {
            _nodes = null;
            _ios?.Clear();
            _ios = null;
        }

        public void OnGUI()
        {
            if (_nodes is null || _ios is null) return;

            if (BTModeManager.IsPlaymode)
            {
                ClearDragNDrop();
            }

            foreach (var io in _ios)
            {
                io.OnGUI();

                if (io.DraggedPin != null && !IsPerformingConnection && !BTModeManager.IsPlaymode)
                {
                    _draggedPin = io.DraggedPin;
                    _draggedIO = io;
                    IsPerformingConnection = true;

                    var mousePin = new Pin(0);
                    performedConnectionDrawer = new PinConnectionDrawer(_draggedPin, mousePin, !_draggedPin.IsInput);

                    Event.current.Use();
                }
                else if (IsPerformingConnection && io.MouseUpPin != null)
                {
                    _mouseUpPin = io.MouseUpPin;
                    _mouseUpIO = io;
                }
                else if (io.MouseDownPin != null)
                {
                    _mouseDownPin = io.MouseDownPin;
                    _mouseDownIO = io;
                }
            }

            if (IsPerformingConnection)
            {
                GUI.changed = true;
            }

            IsConnectionPending = false;
            IsRemovalPending = false;

            if (!BTModeManager.IsPlaymode)
            {
                if (Event.current.type == EventType.MouseDown && BTHotkeys.DeleteConnectionPressed)
                {
                    if (_mouseDownPin != null && _mouseDownIO != null)
                    {
                        if (!_mouseDownPin.IsInput)
                        {
                            RemoveConnection(_mouseDownIO.Node.Index, _mouseDownPin.Index);
                            Event.current.Use();
                            ClearDragNDrop();
                        }
                    }
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (Event.current.button == 0)
                    {
                        if (IsPerformingConnection && _mouseUpPin != null)
                        {
                            if (_mouseUpPin != _draggedPin && _mouseUpIO != _draggedIO)
                            {
                                if (_mouseUpPin.IsInput != _draggedPin.IsInput)
                                {
                                    CreateRemovalBeforeConnection();
                                    CreateConnection();
                                }
                            }

                            IsPerformingConnection = false;
                            Event.current.Use();
                        }
                    }

                    ClearDragNDrop();
                }
            }
            
            if (performedConnectionDrawer != null)
            {
                performedConnectionDrawer.Target.OnGUI(BTEventProcessor.LastRawMousePosition, 0f);
                performedConnectionDrawer.OnGUI();
            }
        }

        private void CreateIO(BehaviourTreeNode node)
        {
            _ios.Add(new NodeDrawerIO(node));
        }

        private void AddConnectionDrawersForIO(NodeDrawerIO io)
        {
            var drawers = new PinConnectionDrawer[io.Node.Connections.Length];

            for (int i = 0; i < drawers.Length; i++)
            {
                int outPinIndex = io.Node.Connections[i].OutPin;
                var outPin = io.OutPins[outPinIndex];
                var inPin = GetIO(io.Node.Connections[i].InNode).InPin;
                drawers[i] = new PinConnectionDrawer(outPin, inPin, true);
                drawers[i].OutPin = outPinIndex;
            }

            io.ConnectionDrawers = drawers;
        }

        private void CreateConnection()
        {
            int outNode = _draggedPin.IsInput ? _mouseUpIO.Node.Index : _draggedIO.Node.Index;
            int inNode = _draggedPin.IsInput ? _draggedIO.Node.Index : _mouseUpIO.Node.Index;
            var pin = _draggedPin.IsInput ? _mouseUpPin : _draggedPin;
            var args = new NodeConnectionEventArgs(outNode, pin.Index, inNode);
            ConnectionPending = args;
            IsConnectionPending = true;
        }

        private void RemoveConnection(int node, int outPin)
        {
            var args = new NodeConnectionEventArgs(node, outPin, 0);
            RemovalPending = args;
            IsRemovalPending = true;
        }

        private void CreateRemovalBeforeConnection()
        {
            ConnectionPin outPin = _draggedPin.IsInput ? _mouseUpPin : _draggedPin;
            var outIO = _draggedPin.IsInput ? _mouseUpIO : _draggedIO;
            bool hasConnectionOnPin = false;

            for (int i = 0; i < outIO.Node.Connections.Length; i++)
            {
                var connection = outIO.Node.Connections[i];

                if (connection.OutPin == outPin.Index)
                {
                    hasConnectionOnPin = true;
                    break;
                }
            }

            if (hasConnectionOnPin)
            {
                RemoveConnection(outIO.Node.Index, outPin.Index);
            }
        }

        private void ClearDragNDrop()
        {
            _draggedPin = null;
            _mouseUpPin = null;
            _draggedIO = null;
            _mouseUpIO = null;
            performedConnectionDrawer = null;
            IsPerformingConnection = false;
        }

        private NodeDrawerIO GetIO(int index)
        {
            return _ios.FirstOrDefault(io => io.Node.Index == index);
        }
    }
}
