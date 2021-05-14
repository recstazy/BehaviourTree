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

        private List<BehaviourTreeNode> nodes;
        private List<NodeDrawerIO> ios;

        private bool isPerformingConnection = false;
        private ConnectionPin draggedPin;
        private ConnectionPin mouseUpPin;
        private ConnectionPin mouseDownPin;

        private NodeDrawerIO draggedIO;
        private NodeDrawerIO mouseUpIO;
        private NodeDrawerIO mouseDownIO;

        private PinConnectionDrawer performedConnectionDrawer;

        #endregion

        #region Properties

        public NodeConnectionEventArgs ConnectionPending { get; private set; }
        public NodeConnectionEventArgs RemovalPending { get; private set; }
        public bool IsRemovalPending { get; private set; }
        public bool IsConnectionPending { get; private set; }
        public bool IsPerformingConnection => isPerformingConnection;

        #endregion

        public NodeConnector(List<BehaviourTreeNode> nodes)
        {
            this.nodes = nodes;
            ios = new List<NodeDrawerIO>();

            foreach (var n in this.nodes)
            {
                CreateIO(n);
            }

            foreach (var io in ios)
            {
                AddConnectionDrawersForIO(io);
            }
        }

        public void Dispose()
        {
            nodes = null;
            ios?.Clear();
            ios = null;
        }

        public void OnGUI()
        {
            if (nodes is null || ios is null) return;

            if (BTModeManager.IsPlaymode)
            {
                ClearDragNDrop();
            }

            foreach (var io in ios)
            {
                io.OnGUI();

                if (io.DraggedPin != null && !isPerformingConnection && !BTModeManager.IsPlaymode)
                {
                    draggedPin = io.DraggedPin;
                    draggedIO = io;
                    isPerformingConnection = true;

                    var mousePin = new Pin(0);
                    performedConnectionDrawer = new PinConnectionDrawer(draggedPin, mousePin, !draggedPin.IsInput);

                    Event.current.Use();
                }
                else if (isPerformingConnection && io.MouseUpPin != null)
                {
                    mouseUpPin = io.MouseUpPin;
                    mouseUpIO = io;
                }
                else if (io.MouseDownPin != null)
                {
                    mouseDownPin = io.MouseDownPin;
                    mouseDownIO = io;
                }
            }

            if (isPerformingConnection)
            {
                GUI.changed = true;
            }

            IsConnectionPending = false;
            IsRemovalPending = false;

            if (!BTModeManager.IsPlaymode)
            {
                if (Event.current.type == EventType.MouseDown && BTHotkeys.DeleteConnectionPressed)
                {
                    if (mouseDownPin != null && mouseDownIO != null)
                    {
                        if (!mouseDownPin.IsInput)
                        {
                            RemoveConnection(mouseDownIO.Node.Index, mouseDownPin.Index);
                            Event.current.Use();
                            ClearDragNDrop();
                        }
                    }
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (Event.current.button == 0)
                    {
                        if (isPerformingConnection && mouseUpPin != null)
                        {
                            if (mouseUpPin != draggedPin && mouseUpIO != draggedIO)
                            {
                                if (mouseUpPin.IsInput != draggedPin.IsInput)
                                {
                                    CreateRemovalBeforeConnection();
                                    CreateConnection();
                                }
                            }

                            isPerformingConnection = false;
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
            ios.Add(new NodeDrawerIO(node));
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
            int outNode = draggedPin.IsInput ? mouseUpIO.Node.Index : draggedIO.Node.Index;
            int inNode = draggedPin.IsInput ? draggedIO.Node.Index : mouseUpIO.Node.Index;
            var pin = draggedPin.IsInput ? mouseUpPin : draggedPin;
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
            ConnectionPin outPin = draggedPin.IsInput ? mouseUpPin : draggedPin;
            var outIO = draggedPin.IsInput ? mouseUpIO : draggedIO;
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
            draggedPin = null;
            mouseUpPin = null;
            draggedIO = null;
            mouseUpIO = null;
            performedConnectionDrawer = null;
            isPerformingConnection = false;
        }

        private NodeDrawerIO GetIO(int index)
        {
            return ios.FirstOrDefault(io => io.Node.Index == index);
        }
    }
}
