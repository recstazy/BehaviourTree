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

        private Vector2 graphPosition;
        private GenericMenu editorMenu;
        private GenericMenu playerMenu;

        private List<BehaviourTreeNode> nodes;
        private BehaviourTreeWindow window;
        private BTNodeSelector selector;
        private BTHotkeys hotkeys;
        private static int selectStartIndexOnConstruct = -1;

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
        public HashSet<int> Selection => selector.Selection;

        #endregion

        public BTEventProcessor(BehaviourTreeWindow window, List<BehaviourTreeNode> nodes)
        {
            this.nodes = nodes;
            this.window = window;
            selector = new BTNodeSelector(nodes);
            CreateMenu();

            BTHotkeys.OnDeleteNodes = () => DeleteNodesPressed = true;
            BTHotkeys.OnCopy = CopySelection;
            BTHotkeys.OnPaste = PasteToGraph;
            BTHotkeys.OnDuplicate = Duplicate;
            hotkeys = new BTHotkeys();

            if (selectStartIndexOnConstruct >= 0)
            {
                var listindex = -1;

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Index == selectStartIndexOnConstruct)
                    {
                        listindex = i;
                        break;
                    }
                }

                selectStartIndexOnConstruct = -1;
                var selectionSet = new BehaviourTreeNode[nodes.Count - listindex];

                for (int i = listindex; i < nodes.Count; i++)
                {
                    selectionSet[i - listindex] = nodes[i];
                }

                selector.ForceSetSelection(selectionSet);
            }
        }

        public void Dispose()
        {
            BTNodeInspector.CloseInspector();
            selector?.Dispose();
            selector = null;
            hotkeys?.Dispose();
            hotkeys = null;
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
            selector.OnGUI();
        }

        public void Process(Event e)
        {
            LastRawMousePosition = Event.current.mousePosition;
            LastMousePosition = LastRawMousePosition + EditorZoomer.ContentOffset;
            DeleteNodesPressed = false;

            if (!BTModeManager.IsPlaymode)
            {
                if (hotkeys.Process(e))
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
            var menu = BTModeManager.IsPlaymode ? playerMenu : editorMenu;
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void CreateMenu()
        {
            editorMenu = new GenericMenu();
            editorMenu.AddItem(new GUIContent("Add Node"), false, CallCreateNode);

            playerMenu = new GenericMenu();
            playerMenu.AddDisabledItem(new GUIContent("Add Node"));
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
            selector.ProcessEvent(e);
            bool shouldCloseInspector = true;

            if (selector.DraggedAnyNode)
            {
                OnNodesMoved?.Invoke(selector.Selection);
                return true;
            }
            else if (selector.ClickedNodeListIndex >= 0)
            {
                if (selector.GetWasDoubleClickAndClear())
                {
                    if (!BTNodeInspector.IsActive)
                    {
                        BTNodeInspector.SetupForSelection(nodes[selector.ClickedNodeListIndex], window);
                        shouldCloseInspector = false;
                    }
                }
            }
            else if (selector.MouseDownNodeIndex == BTNodeInspector.CurrentNodeIndex)
            {
                shouldCloseInspector = false;
            }

            if (selector.EventUsed)
            {
                if (shouldCloseInspector)
                {
                    BTNodeInspector.CloseInspector();
                }
            }

            return selector.EventUsed;
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
            selectStartIndexOnConstruct = NodeCopier.LastPasteStartIndex;
            OnRegisterUndoAndSetDirty?.Invoke(undoName);
            OnUpdateNodes?.Invoke();
        }

        private void StartZoomArea(Rect windowRect)
        {

        }
    }
}
