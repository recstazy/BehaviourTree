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

        private List<BehaviourTreeNode> _nodes;
        private bool _isDraggingNodes;
        private Rect _selectionRect;
        private bool _isMultiSelecting;
        private bool _isDraggingMouse;
        private Vector2 _mouseDownPos;
        private int _lastClickedNode;
        private bool _wasDoubleClick;

        #endregion

        #region Properties

        public int MouseDownNodeListIndex { get; private set; }
        public int MouseUpNodeListIndex { get; private set; }
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
            _nodes = nodes;
        }

        public void Dispose()
        {
            UpdateNodesSelection(null);
            _nodes = null;
            Selection = null;
        }

        public void ClearClickInfo()
        {
            SetMouseDown(-1, -1);
            SetMouseUp(-1, -1);
            SetMouseClick(-1, -1);
            _lastClickedNode = -1;
            _wasDoubleClick = false;
            _isDraggingMouse = false;
            _isDraggingNodes = false;
        }

        public bool GetWasDoubleClickAndClear()
        {
            bool wasDoubleClick = _wasDoubleClick;
            _wasDoubleClick = false;
            return wasDoubleClick;
        }

        public void ForceSetSelection(params BehaviourTreeNode[] nodes)
        {
            if (nodes == null || nodes.Length == 0) return;

            var listIndices = nodes.Select(n => _nodes.IndexOf(n)).ToArray();
            _isMultiSelecting = nodes.Length > 1;
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
            _wasDoubleClick = false;
            if (e.button != 0) return;

            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        MouseDown();
                        ProcessNodesClickInfo();

                        e.Use();
                        EventUsed = true;

                        _isDraggingMouse = false;
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

                        _isDraggingMouse = false;
                        GUI.changed = true;
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        _isDraggingMouse = true;
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

            _mouseDownPos = BTEventProcessor.LastRawMousePosition;
            _selectionRect.position = _mouseDownPos;
        }

        private void MouseUp()
        {
            if (!_isDraggingNodes)
            {
                var mouseUpNode = GetNodeUnderCursor(out var node);
                var clickedNodeListIndex = mouseUpNode == MouseDownNodeListIndex ? mouseUpNode : -1;
                _wasDoubleClick = !_isDraggingMouse && _lastClickedNode == clickedNodeListIndex;
                _lastClickedNode = clickedNodeListIndex;

                SetMouseUp(mouseUpNode, node is null ? -1 : node.Index);
                SetMouseClick(clickedNodeListIndex, clickedNodeListIndex >= 0 ? node.Index : -1);
            }
            else
            {
                foreach (var n in Selection)
                {
                    _nodes[n].EndDrag();
                }
            }
            
            if (_isDraggingMouse)
            {
                if (Selection.Count > 0 && MouseDownNodeListIndex >= 0)
                {
                    DraggedAnyNode = true;
                }
            }

            _selectionRect.size = Vector2.zero;
            _isDraggingNodes = false;

            if (!_isDraggingMouse)
            {
                _isMultiSelecting = false;
            }
        }

        private void MouseDrag(Vector2 delta)
        {
            if (MouseDownNodeListIndex >= 0)
            {
                if (!Selection.Contains(MouseDownNodeListIndex))
                {
                    UpdateNodesSelection(MouseDownNodeListIndex);
                }

                _isDraggingNodes = true;
                DragSelected(delta);
            }
            else
            {
                SelectionRectUpdate(delta);
                _isMultiSelecting = true;
            }
        }

        private bool ProcessNodesClickInfo()
        {
            if (ClickedNodeListIndex >= 0)
            {
                if (!_isMultiSelecting)
                {
                    UpdateNodesSelection(ClickedNodeListIndex);
                }

                return true;
            }
            else
            {
                if (!_isMultiSelecting)
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
                _nodes[n].Drag(delta);
            }
        }

        private void SelectionRectUpdate(Vector2 delta)
        {
            ChangeSelectionRect(delta);
            var nodesInRect = new List<int>();

            for(int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].GetTransformedRect().Overlaps(_selectionRect))
                {
                    nodesInRect.Add(i);
                }
            }

            UpdateNodesSelection(nodesInRect.ToArray());
        }

        private void ChangeSelectionRect(Vector2 delta)
        {
            Vector2 curremtMousePos = BTEventProcessor.LastRawMousePosition;
            Vector2 mouseDownToCurrent = curremtMousePos - _mouseDownPos;

            if (mouseDownToCurrent.x > 0 && mouseDownToCurrent.y > 0)
            {
                _selectionRect.position = _mouseDownPos;
                _selectionRect.size = mouseDownToCurrent;
            }
            else if (mouseDownToCurrent.x > 0 && mouseDownToCurrent.y <= 0)
            {
                _selectionRect.y = curremtMousePos.y;
                _selectionRect.height = -mouseDownToCurrent.y;
                _selectionRect.width = mouseDownToCurrent.x;
            }
            else if (mouseDownToCurrent.x <= 0 && mouseDownToCurrent.y > 0)
            {
                _selectionRect.x = curremtMousePos.x;
                _selectionRect.height = mouseDownToCurrent.y;
                _selectionRect.width = -mouseDownToCurrent.x;
            }
            else if (mouseDownToCurrent.x <= 0 && mouseDownToCurrent.y <= 0)
            {
                _selectionRect.position = curremtMousePos;
                _selectionRect.size = -mouseDownToCurrent;
            }
        }

        private void UpdateNodesSelection(params int[] newSelectedNodes)
        {
            foreach (var s in Selection)
            {
                if (s >= 0 && s < _nodes.Count)
                {
                    _nodes[s].Selected = false;
                }
            }

            if (newSelectedNodes == null)
            {
                Selection.Clear();
            }
            else
            {
                foreach (var s in newSelectedNodes)
                {
                    _nodes[s].Selected = true;
                }

                Selection = new HashSet<int>(newSelectedNodes);
            }

            GUI.changed = true;
        }

        private int GetNodeUnderCursor(out BehaviourTreeNode node)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].MainRect.Contains(BTEventProcessor.LastMousePosition))
                {
                    node = _nodes[i];
                    return i;
                }
            }

            node = null;
            return -1;
        }

        private void DrawSelectionRect()
        {
            if (_isMultiSelecting)
            {
                EditorGUI.DrawRect(_selectionRect, new Color(0.1f, 0.1f, 0.1f, 0.1f));
            }
        }

        private void SetMouseDown(int listIndex, int nodeIndex)
        {
            MouseDownNodeListIndex = listIndex;
            MouseDownNodeIndex = nodeIndex;
        }

        private void SetMouseUp(int listIndex, int nodeIndex)
        {
            MouseUpNodeListIndex = listIndex;
            MouseUpNodeIndex = nodeIndex;
        }

        private void SetMouseClick(int listIndex, int nodeIndex)
        {
            ClickedNodeListIndex = listIndex;
            ClickNodeIndex = nodeIndex;
        }
    }
}
