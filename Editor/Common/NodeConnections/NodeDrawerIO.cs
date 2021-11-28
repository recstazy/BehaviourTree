using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class NodeDrawerIO
    {
        #region Fields

        private static readonly Vector2 s_bottomBorder = Vector2.up * 0.5f;
        private int _outsCount = 0;
        private bool _isEntryNode;
        private Vector2[] _outsPositions;

        #endregion

        #region Properties

        public BehaviourTreeNode Node { get; }
        public ConnectionPin DraggedPin = null;
        public ConnectionPin MouseDownPin = null;
        public ConnectionPin MouseUpPin = null;
        public bool WasMouseUpOnNodeRect { get; private set; }
        public PinConnectionDrawer[] ConnectionDrawers { get; set; }

        public ConnectionPin InPin { get; private set; }
        public ConnectionPin[] OutPins { get; private set; }

        #endregion

        public NodeDrawerIO(BehaviourTreeNode node)
        {
            Node = node;
            _isEntryNode = node is EntryTreeNode;

            if (!_isEntryNode)
            {
                InPin = new ConnectionPin(0, true);
                InPin.GetMousePosition = () => BTEventProcessor.LastRawMousePosition;
            }

            var outAttributes = node.GetOuts();
            _outsCount = outAttributes is null ? 0 : outAttributes.Length;
            OutPins = new ConnectionPin[_outsCount];
            _outsPositions = new Vector2[OutPins.Length];

            for (int i = 0; i < _outsCount; i++)
            {
                var attribute = outAttributes[i];
                int index = attribute.Index;

                OutPins[index] = new ConnectionPin(index, false, attribute.Name);
                OutPins[index].GetMousePosition = () => BTEventProcessor.LastRawMousePosition;
                OutPins[index].Name = attribute.Name;
            }
        }

        public void OnGUI()
        {
            WasMouseUpOnNodeRect = false;
            DraggedPin = null;
            MouseDownPin = null;
            MouseUpPin = null;
            var nodeRect = Node.GetTransformedRect();

            if (ConnectionDrawers != null)
            {
                foreach (var drawer in ConnectionDrawers)
                {
                    drawer.OnGUI();
                }
            }
            
            if (!_isEntryNode)
            {
                InPin.OnGUI(GetInPosition(nodeRect), nodeRect.width);
                ProcessPinEvents(InPin);

                if (Event.current.type == EventType.MouseUp)
                {
                    if (nodeRect.Contains(BTEventProcessor.LastRawMousePosition))
                    {
                        WasMouseUpOnNodeRect = true;
                    }
                }
            }

            float pinWidth = nodeRect.width / OutPins.Length;
            UpdateOutsPositions(nodeRect, pinWidth);
            
            for (int i = 0; i < OutPins.Length; i++)
            {
                OutPins[i].OnGUI(_outsPositions[i], pinWidth);
                ProcessPinEvents(OutPins[i]);
            }
        }

        private void ProcessPinEvents(ConnectionPin pin)
        {
            if (pin.MouseDrag)
            {
                DraggedPin = pin;
            }
            else if (pin.MouseUp)
            {
                MouseUpPin = pin;
            }

            if (pin.MouseDown)
            {
                MouseDownPin = pin;
            }
        }

        private Vector2 GetInPosition(Rect nodeRect)
        {
            return nodeRect.center + nodeRect.height * (-s_bottomBorder);
        }

        private void UpdateOutsPositions(Rect nodeRect, float pinWidth)
        {
            var pinsCount = _outsPositions.Length;
            Vector2 startPosition = nodeRect.position + new Vector2(pinWidth * 0.5f, nodeRect.height);

            for (int i = 0; i < pinsCount; i++)
            {
                _outsPositions[i] = startPosition + Vector2.right * pinWidth * i;
            }
        }
    }
}
