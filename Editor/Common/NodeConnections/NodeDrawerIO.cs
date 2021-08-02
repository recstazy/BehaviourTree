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

        private static readonly Vector2 bottomBorder = Vector2.up * 0.5f;

        private BehaviourTreeNode node;
        private int outsCount = 0;
        private bool isEntryNode;
        private Vector2[] outsPositions;

        #endregion

        #region Properties

        public BehaviourTreeNode Node => node;
        public ConnectionPin DraggedPin = null;
        public ConnectionPin MouseDownPin = null;
        public ConnectionPin MouseUpPin = null;
        public PinConnectionDrawer[] ConnectionDrawers { get; set; }

        public ConnectionPin InPin { get; private set; }
        public ConnectionPin[] OutPins { get; private set; }

        #endregion

        public NodeDrawerIO(BehaviourTreeNode node)
        {
            this.node = node;
            isEntryNode = node is EntryTreeNode;

            if (!isEntryNode)
            {
                InPin = new ConnectionPin(0, true);
                InPin.GetMousePosition = () => BTEventProcessor.LastRawMousePosition;
            }

            var outAttributes = node.GetOuts();
            outsCount = outAttributes is null ? 0 : outAttributes.Length;
            OutPins = new ConnectionPin[outsCount];
            outsPositions = new Vector2[OutPins.Length];

            for (int i = 0; i < outsCount; i++)
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
            DraggedPin = null;
            MouseDownPin = null;
            MouseUpPin = null;
            var nodeRect = node.GetTransformedRect();

            if (ConnectionDrawers != null)
            {
                foreach (var drawer in ConnectionDrawers)
                {
                    drawer.OnGUI();
                }
            }
            
            if (!isEntryNode)
            {
                InPin.OnGUI(GetInPosition(nodeRect), nodeRect.width);
                ProcessPinEvents(InPin);
            }

            float pinWidth = nodeRect.width / OutPins.Length;
            UpdateOutsPositions(nodeRect, pinWidth);
            
            for (int i = 0; i < OutPins.Length; i++)
            {
                OutPins[i].OnGUI(outsPositions[i], pinWidth);
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
            return nodeRect.center + nodeRect.height * (-bottomBorder);
        }

        private void UpdateOutsPositions(Rect nodeRect, float pinWidth)
        {
            var pinsCount = outsPositions.Length;
            Vector2 startPosition = nodeRect.position + new Vector2(pinWidth * 0.5f, nodeRect.height);

            for (int i = 0; i < pinsCount; i++)
            {
                outsPositions[i] = startPosition + Vector2.right * pinWidth * i;
            }
        }
    }
}
