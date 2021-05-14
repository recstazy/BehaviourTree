using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNodeSelector
    {
        #region Fields

        private List<BehaviourTreeNode> nodes;
        private int mouseDownNode;
        private int mouseUpNode;
        private bool isDraggingNodes;
        private Rect selectionRect;
        private bool isMultiSelecting;
        private bool isDraggingMouse;
        private Vector2 mouseDownPos;
        private int lastClickedNode;
        private bool wasDoubleClick;

        #endregion

        #region Properties

        public int MouseDownNodeListIndex => mouseDownNode;
        public int MouseUpNodeListIndex => mouseUpNode;
        public int ClickedNodeListIndex { get; private set; }

        public int MouseDownNodeIndex { get; private set; }
        public int MouseUpNodeIndex { get; private set; }
        public int ClickNodeIndex { get; private set; }

        public bool EventUsed { get; private set; }
        public bool DraggedAnyNode { get; private set; }
        public HashSet<int> Selection { get; private set; } = new HashSet<int>();

        #endregion

        public BTNodeSelector(List<BehaviourTreeNode> nodes)
        {
            this.nodes = nodes;
        }

        public void Dispose()
        {
            UpdateNodesSelection(null);
            nodes = null;
            Selection = null;
        }

        public void ClearClickInfo()
        {
            SetMouseDown(-1, -1);
            SetMouseUp(-1, -1);
            SetMouseClick(-1, -1);
            lastClickedNode = -1;
            wasDoubleClick = false;
            isDraggingMouse = false;
            isDraggingNodes = false;
        }

        public bool GetWasDoubleClickAndClear()
        {
            bool wasDoubleClick = this.wasDoubleClick;
            this.wasDoubleClick = false;
            return wasDoubleClick;
        }

        public void ForceSetSelection(params BehaviourTreeNode[] nodes)
        {
            if (nodes is null || nodes.Length == 0) return;

            var listIndices = nodes.Select(n => this.nodes.IndexOf(n)).ToArray();
            isMultiSelecting = nodes.Length > 1;
            UpdateNodesSelection(listIndices);
        }

        public void OnGUI()
        {
            DrawSelectionRect();
        }

        public void ProcessEvent(Event e)
        {
            SetMouseClick(-1, -1);
            EventUsed = false;
            DraggedAnyNode = false;
            wasDoubleClick = false;
            if (e.button != 0) return;

            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        MouseDown();
                        ProcessNodesClickInfo();

                        e.Use();
                        EventUsed = true;

                        isDraggingMouse = false;
                        GUI.changed = true;
                        break;
                    }
                case EventType.MouseUp:
                    {
                        MouseUp();

                        if (ProcessNodesClickInfo())
                        {
                            e.Use();
                            EventUsed = true;
                        }

                        isDraggingMouse = false;
                        GUI.changed = true;
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        isDraggingMouse = true;
                        MouseDrag(e.delta);
                        GUI.changed = true;
                        break;
                    }
            }
        }

        private void MouseDown()
        {
            var mouseDownNode = GetNodeUnderCursor(out var node);
            SetMouseDown(mouseDownNode, node is null ? -1 : node.Index);
            SetMouseUp(-1, -1);
            SetMouseClick(-1, -1);

            mouseDownPos = BTEventProcessor.LastRawMousePosition;
            selectionRect.position = mouseDownPos;
        }

        private void MouseUp()
        {
            if (!isDraggingNodes)
            {
                var mouseUpNode = GetNodeUnderCursor(out var node);
                var clickedNodeListIndex = mouseUpNode == mouseDownNode ? mouseUpNode : -1;
                wasDoubleClick = !isDraggingMouse && lastClickedNode == clickedNodeListIndex;
                lastClickedNode = clickedNodeListIndex;

                SetMouseUp(mouseUpNode, node is null ? -1 : node.Index);
                SetMouseClick(clickedNodeListIndex, clickedNodeListIndex >= 0 ? node.Index : -1);
            }
            
            if (isDraggingMouse)
            {
                if (Selection.Count > 0 && mouseDownNode >= 0)
                {
                    DraggedAnyNode = true;
                }
            }

            selectionRect.size = Vector2.zero;
            isDraggingNodes = false;

            if (!isDraggingMouse)
            {
                isMultiSelecting = false;
            }
        }

        private void MouseDrag(Vector2 delta)
        {
            if (mouseDownNode >= 0)
            {
                if (!Selection.Contains(mouseDownNode))
                {
                    UpdateNodesSelection(mouseDownNode);
                }

                DragSelected(delta);
            }
            else
            {
                SelectionRectUpdate(delta);
                isMultiSelecting = true;
            }
        }

        private bool ProcessNodesClickInfo()
        {
            if (ClickedNodeListIndex >= 0)
            {
                if (!isMultiSelecting)
                {
                    UpdateNodesSelection(ClickedNodeListIndex);
                }

                return true;
            }
            else
            {
                if (!isMultiSelecting)
                {
                    UpdateNodesSelection(null);
                }
            }

            return false;
        }

        private void DragSelected(Vector2 delta)
        {
            foreach (var n in Selection)
            {
                nodes[n].Drag(delta);
            }
        }

        private void SelectionRectUpdate(Vector2 delta)
        {
            ChangeSelectionRect(delta);
            var nodesInRect = new List<int>();

            for(int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GetTransformedRect().Overlaps(selectionRect))
                {
                    nodesInRect.Add(i);
                }
            }

            UpdateNodesSelection(nodesInRect.ToArray());
        }

        private void ChangeSelectionRect(Vector2 delta)
        {
            Vector2 curremtMousePos = BTEventProcessor.LastRawMousePosition;
            Vector2 mouseDownToCurrent = curremtMousePos - mouseDownPos;

            if (mouseDownToCurrent.x > 0 && mouseDownToCurrent.y > 0)
            {
                selectionRect.position = mouseDownPos;
                selectionRect.size = mouseDownToCurrent;
            }
            else if (mouseDownToCurrent.x > 0 && mouseDownToCurrent.y <= 0)
            {
                selectionRect.y = curremtMousePos.y;
                selectionRect.height = -mouseDownToCurrent.y;
                selectionRect.width = mouseDownToCurrent.x;
            }
            else if (mouseDownToCurrent.x <= 0 && mouseDownToCurrent.y > 0)
            {
                selectionRect.x = curremtMousePos.x;
                selectionRect.height = mouseDownToCurrent.y;
                selectionRect.width = -mouseDownToCurrent.x;
            }
            else if (mouseDownToCurrent.x <= 0 && mouseDownToCurrent.y <= 0)
            {
                selectionRect.position = curremtMousePos;
                selectionRect.size = -mouseDownToCurrent;
            }
        }

        private void UpdateNodesSelection(params int[] newSelectedNodes)
        {
            foreach (var s in Selection)
            {
                if (s >= 0 && s < nodes.Count)
                {
                    nodes[s].Selected = false;
                }
            }

            if (newSelectedNodes is null)
            {
                Selection.Clear();
            }
            else
            {
                foreach (var s in newSelectedNodes)
                {
                    nodes[s].Selected = true;
                }

                Selection = new HashSet<int>(newSelectedNodes);
            }

            GUI.changed = true;
        }

        private int GetNodeUnderCursor(out BehaviourTreeNode node)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].MainRect.Contains(BTEventProcessor.LastMousePosition))
                {
                    node = nodes[i];
                    return i;
                }
            }

            node = null;
            return -1;
        }

        private void DrawSelectionRect()
        {
            if (isMultiSelecting)
            {
                EditorGUI.DrawRect(selectionRect, new Color(0.1f, 0.1f, 0.1f, 0.1f));
            }
        }

        private void SetMouseDown(int listINdex, int nodeIndex)
        {
            mouseDownNode = listINdex;
            MouseDownNodeIndex = nodeIndex;
        }

        private void SetMouseUp(int listINdex, int nodeIndex)
        {
            mouseUpNode = listINdex;
            MouseUpNodeIndex = nodeIndex;
        }

        private void SetMouseClick(int listINdex, int nodeIndex)
        {
            ClickedNodeListIndex = listINdex;
            ClickNodeIndex = nodeIndex;
        }
    }
}
