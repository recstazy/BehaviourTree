using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTEventProcessor
    {

        #region Fields

        private GenericMenu _editorMenu;
        private GenericMenu _playerMenu;

        private List<BehaviourTreeNode> _nodes;
        private BehaviourTreeWindow _window;
        private BTNodeSelector _selector;
        private BTHotkeys _hotkeys;
        private static int s_selectStartIndexOnConstruct = -1;

        #endregion

        #region Properties

        public static Vector2 CurrentGraphPosition { get => EditorZoomer.ZoomOrigin; set => EditorZoomer.ZoomOrigin = value; }
        public static Vector2 LastMousePosition { get; private set; }
        public static Vector2 LastRawMousePosition { get; private set; }

        public Action OnUndo { get; set; }
        public Action OnSetDirty { get; set; }
        public Action OnUpdateNodes { get; set; }
        public Action<string> OnRegisterUndoAndSetDirty { get; set; }
        public Action OnCreateNode { get; set; }
        public Action<HashSet<int>> OnCopy { get; set; }
        public Func<bool> OnPaste { get; set; }
        public Func<HashSet<int>, bool> OnDuplicate { get; set; }
        public Action<HashSet<int>> OnNodesMoved { get; set; }

        public bool CanSelect { get; set; }
        public bool DeleteNodesPressed { get; private set; }
        public HashSet<int> Selection => _selector.Selection;

        #endregion

        public BTEventProcessor(BehaviourTreeWindow window, List<BehaviourTreeNode> nodes)
        {
            _nodes = nodes;
            _window = window;
            _selector = new BTNodeSelector(nodes);
            CreateMenu();

            BTHotkeys.OnDeleteNodes = () => DeleteNodesPressed = true;
            BTHotkeys.OnCopy = CopySelection;
            BTHotkeys.OnPaste = PasteToGraph;
            BTHotkeys.OnDuplicate = Duplicate;
            _hotkeys = new BTHotkeys();

            if (s_selectStartIndexOnConstruct >= 0)
            {
                var listindex = -1;

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Index == s_selectStartIndexOnConstruct)
                    {
                        listindex = i;
                        break;
                    }
                }

                s_selectStartIndexOnConstruct = -1;
                var selectionSet = new BehaviourTreeNode[nodes.Count - listindex];

                for (int i = listindex; i < nodes.Count; i++)
                {
                    selectionSet[i - listindex] = nodes[i];
                }

                _selector.ForceSetSelection(selectionSet);
            }
        }

        public void Dispose()
        {
            BTNodeInspector.CloseInspector();
            _selector?.Dispose();
            _selector = null;
            _hotkeys?.Dispose();
            _hotkeys = null;
        }

        public void BeginZoom()
        {
            EditorZoomer.Begin();
        }

        public void EndZoom()
        {
            EditorZoomer.End();
        }

        public void OnGUI()
        {
            _selector.OnGUI();
        }

        public void Process(Event e)
        {
            LastRawMousePosition = Event.current.mousePosition;
            LastMousePosition = LastRawMousePosition + EditorZoomer.ContentOffset;
            DeleteNodesPressed = false;

            if (!BTModeManager.IsPlaymode)
            {
                if (_hotkeys.Process(e))
                {
                    return;
                }

                if (CanSelect)
                {
                    if (ProcessNodeSelection(e))
                    {
                        return;
                    }
                }
            }

            ProcessOtherEvents(e);
        }

        private void ProcessContextClick()
        {
            var menu = BTModeManager.IsPlaymode ? _playerMenu : _editorMenu;
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void CreateMenu()
        {
            _editorMenu = new GenericMenu();
            _editorMenu.AddItem(new GUIContent("Add Node"), false, CallCreateNode);

            _playerMenu = new GenericMenu();
            _playerMenu.AddDisabledItem(new GUIContent("Add Node"));
        }

        private void CallCreateNode()
        {
            OnCreateNode?.Invoke();
        }

        private void ProcessOtherEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 1)
                        {
                            ProcessContextClick();
                            e.Use();
                        }

                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (e.button == 2)
                        {
                            if (EditorZoomer.PanOrZoomChanged)
                            {
                                OnSetDirty?.Invoke();
                            }

                            e.Use();
                        }

                        break;
                    }
                case EventType.ScrollWheel:
                    {
                        if (EditorZoomer.PanOrZoomChanged)
                        {
                            OnSetDirty?.Invoke();
                        }

                        e.Use();
                        break;
                    }
                case EventType.ValidateCommand:
                    {
                        if (string.Equals(e.commandName, "UndoRedoPerformed"))
                        {
                            OnUndo?.Invoke();
                            GUI.changed = true;
                            e.Use();
                        }

                        break;
                    }
            }
        }

        private bool ProcessNodeSelection(Event e)
        {
            _selector.ProcessEvent(e);
            bool shouldCloseInspector = true;

            if (_selector.DraggedAnyNode)
            {
                OnNodesMoved?.Invoke(_selector.Selection);
                return true;
            }
            else if (_selector.ClickedNodeListIndex >= 0)
            {
                if (_selector.GetWasDoubleClickAndClear())
                {
                    if (!BTNodeInspector.IsActive)
                    {
                        BTNodeInspector.SetupForSelection(_nodes[_selector.ClickedNodeListIndex], _window);
                        shouldCloseInspector = false;
                    }
                }
            }
            else if (_selector.MouseDownNodeIndex == BTNodeInspector.CurrentNodeIndex)
            {
                shouldCloseInspector = false;
            }

            if (_selector.EventUsed)
            {
                if (shouldCloseInspector)
                {
                    BTNodeInspector.CloseInspector();
                }
            }

            return _selector.EventUsed;
        }

        private void Duplicate()
        {
            bool canDuplicate = OnDuplicate != null;

            if (canDuplicate)
            {
                if (OnDuplicate.Invoke(Selection))
                {
                    AfterNodesPasteComplete("Duplicate Nodes");
                }
            }
        }

        private void CopySelection()
        {
            OnCopy?.Invoke(Selection);
        }

        private void PasteToGraph()
        {
            bool pasteSuccess = OnPaste is null ? false : OnPaste.Invoke();

            if (pasteSuccess)
            {
                AfterNodesPasteComplete("Copy Paste Nodes");
            }
        }

        private void AfterNodesPasteComplete(string undoName)
        {
            s_selectStartIndexOnConstruct = NodeCopier.LastPasteStartIndex;
            OnRegisterUndoAndSetDirty?.Invoke(undoName);
            OnUpdateNodes?.Invoke();
        }
    }
}
