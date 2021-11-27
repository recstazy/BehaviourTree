using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BehaviourTreeWindow : EditorWindow
    {
        #region Fields

        private int _lastAssetID;
        private BTEventProcessor _eventProcessor;
        private NodeConnector _connector;
        private BTModeManager _modeManager;
        private BTTargetWatcher _watcher;
        private BTNodeProcessor _nodeProcessor;
        private BTSnapManager _snapManager;
        private bool _isInitialized;
        private static readonly Color s_topBarColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        private const float TopBarHeight = 20f;

        #endregion

        #region Properties

        public static Vector2 GraphPosition => BTEventProcessor.CurrentGraphPosition;
        public BehaviourTree TreeInstance { get; private set; }
        public BehaviourTree TreeAsset { get; private set; }

        private string Title { set => titleContent = new GUIContent($"Tree: {value}"); }

        #endregion

        public static void ShowWindow(BehaviourTree treeAsset)
        {
            var window = GetWindow<BehaviourTreeWindow>(false, $"Tree: {treeAsset.name}", true);
            window.InitializeWithAsset(treeAsset);
        }

        [OnOpenAsset(2)]
        public static bool AssetOpened(int instanceID, int line)
        {
            var assetObject = EditorUtility.InstanceIDToObject(instanceID);

            if (assetObject is BehaviourTree treeAsset)
            {
                ShowWindow(treeAsset);
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            if (_lastAssetID != 0)
            {
                var assetObject = EditorUtility.InstanceIDToObject(_lastAssetID);

                if (assetObject is BehaviourTree treeAsset)
                {
                    if (TreeInstance is null || TreeAsset != treeAsset)
                    {
                        InitializeWithAsset(treeAsset);
                    }
                }
            }

            BTTargetWatcher.UpdateIsPlaymode();
            EditorApplication.update += EditorUpdate;
            TaskFactory.UpdateTaskTypes();
            NodeDrawerProvider.UpdateTaskDrawers();
            Undo.IncrementCurrentGroup();
        }

        private void OnDisable()
        {
            Dispose();
            EditorApplication.update -= EditorUpdate;
            Undo.IncrementCurrentGroup();
        }

        private void OnGUI()
        {
            DrawTopBar();

            if (_isInitialized)
            {
                _watcher.OnGUI(position);
                if (ProcessWatcherEvents()) return;

                if (!BTModeManager.IsPlaymode) _snapManager.OnGUI(position);

                _eventProcessor.BeginZoom();
                _connector.OnGUI();
                ProcessConnectorShedules();
                _nodeProcessor.OnNodesGUI();

                _eventProcessor.CanSelect = !_connector.IsPerformingConnection;
                _eventProcessor.OnGUI();
                _eventProcessor.Process(Event.current);

                if (_eventProcessor.DeleteNodesPressed)
                {
                    if (_nodeProcessor.DeleteNodesBySelection(_eventProcessor.Selection))
                    {
                        UpdateTreeAssetAndSetDirty("Delete Nodes");
                        _nodeProcessor.ClearNodesDirty();
                        ReinitNodesDependencies();
                    }
                }
                else
                {
                    SetAssetDirtyIfNodesDirty();
                }

                _eventProcessor.EndZoom();
                _modeManager.OnGUI(position);
            }
        }

        private void EditorUpdate()
        {
            if (_isInitialized)
            {
                if (BTModeManager.IsPlaymode)
                {
                    if (_nodeProcessor.UpdateRunningNodesAndCheckChanged())
                    {
                        GUI.changed = true;
                    }
                }

                if (GUI.changed)
                {
                    Repaint();
                }
            }
        }

        private void InitializeWithAsset(BehaviourTree tree)
        {
            if (TreeInstance != null) Dispose();
            if (tree == null) return;
            if (!tree.IsRuntime) _lastAssetID = tree.GetInstanceID();

            TreeAsset = tree;
            Title = TreeAsset.name;
            InitializeTreeAssetIfEmpty();

            BTNodeInspector.OnClosed += UpdateAssetAfterNodeInspector;
            TaskFactory.UpdateTaskTypes();
            NodeDrawerProvider.UpdateTaskDrawers();
            _modeManager = new BTModeManager();
            _watcher = new BTTargetWatcher();
            _nodeProcessor = new BTNodeProcessor();
            _snapManager = new BTSnapManager(TreeAsset.SnapEnabled);
            FetchGrapghFromAsset();
            _isInitialized = true;
        }

        private void InitializeTreeAssetIfEmpty()
        {
            if (TreeAsset.EntryNode is null)
            {
                TreeAsset.CreateEntry();
                EditorUtility.SetDirty(TreeAsset);
            }
        }

        private void FetchGrapghFromAsset()
        {
            TreeInstance = TreeAsset.IsRuntime ? TreeAsset : Instantiate(TreeAsset);
            _nodeProcessor.RecreateNodes(TreeInstance);
            ReinitNodesDependencies();
        }

        private void Dispose()
        {
            SetDirtyAndSaveSaveAll();
            TreeInstance = null;
            TreeAsset = null;
            _nodeProcessor?.Dispose();
            _nodeProcessor = null;
            _eventProcessor?.Dispose();
            _eventProcessor = null;
            _connector?.Dispose();
            _connector = null;
            _modeManager?.Dispose();
            _modeManager = null;
            _watcher?.Dispose();
            _watcher = null;
            BTNodeInspector.OnClosed -= UpdateAssetAfterNodeInspector;
            BTNodeInspector.CloseInspector();
            _isInitialized = false;
        }

        private void SetDirtyAndSaveSaveAll()
        {
            UpdateTreeAssetAndSetDirty();
            SaveAssets();
        }

        private void SaveAssets()
        {
            if (!TreeInstance.IsRuntime)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void UpdateAssetAfterNodeInspector()
        {
            if (TreeAsset != null && TreeInstance != null)
            {
                UpdateTreeAssetAndSetDirty("Edit Node Task");
            }
        }

        private void UpdateTreeAssetAndSetDirty(string undoName)
        {
            Undo.RecordObject(TreeAsset, undoName);
            UpdateTreeAssetAndSetDirty();
        }

        private void UpdateTreeAssetAndSetDirty()
        {
            TreeAsset.NodeData = new TreeNodeData(_nodeProcessor.Nodes.Select(n => n.Data).ToArray());
            TreeAsset.GraphPosition = BTEventProcessor.CurrentGraphPosition;
            TreeAsset.Zoom = EditorZoomer.Zoom;
            TreeAsset.SnapEnabled = BTSnapManager.SnapEnabled;

            if (!TreeInstance.IsRuntime)
            {
                EditorUtility.SetDirty(TreeAsset);
            }
        }

        private void CreateNewNodeInEditor()
        {
            _nodeProcessor.CreateNewNodeInEditor();
            UpdateTreeAssetAndSetDirty("Add Node");
            FetchGrapghFromAsset();
        }

        private void SetAssetDirtyIfNodesDirty()
        {
            if (_nodeProcessor.NodesDirty)
            {
                _nodeProcessor.ClearNodesDirty();
                UpdateTreeAssetAndSetDirty("Change Node Task");
                FetchGrapghFromAsset();
            }
        }

        private void ReinitNodesDependencies()
        {
            _connector?.Dispose();
            _connector = new NodeConnector(_nodeProcessor.Nodes);

            _eventProcessor?.Dispose();
            _eventProcessor = new BTEventProcessor(this, _nodeProcessor.Nodes);
            BTEventProcessor.CurrentGraphPosition = TreeAsset.GraphPosition;
            EditorZoomer.Zoom = TreeAsset.Zoom;
            _eventProcessor.OnUndo = FetchGrapghFromAsset;
            _eventProcessor.OnSetDirty = UpdateTreeAssetAndSetDirty;
            _eventProcessor.OnRegisterUndoAndSetDirty = UpdateTreeAssetAndSetDirty;
            _eventProcessor.OnCreateNode = CreateNewNodeInEditor;
            _eventProcessor.OnCopy = _nodeProcessor.CopySelectedToClipboard;
            _eventProcessor.OnPaste = _nodeProcessor.CreateNewNodesFromClipboardUnderMouse;
            _eventProcessor.OnDuplicate = _nodeProcessor.DuplicateNodes;
            _eventProcessor.OnUpdateNodes = FetchGrapghFromAsset;
            _eventProcessor.OnNodesMoved = NodesMoved;

            GUI.changed = true;
            Repaint();
        }

        private void ProcessConnectorShedules()
        {
            bool changed = _connector.IsRemovalPending || _connector.IsConnectionPending;
            string undoMessage = null;

            if (changed)
            {
                if (_connector.IsConnectionPending)
                {
                    undoMessage = "Create Connection";
                }
                else
                {
                    undoMessage = "Remove Connection";
                }
            }

            if (_connector.IsRemovalPending)
            {
                _nodeProcessor.RemoveConnection(_connector.RemovalPending);
            }

            if (_connector.IsConnectionPending)
            {
                _nodeProcessor.CreateConnection(_connector.ConnectionPending);
            }

            if (changed)
            {
                UpdateTreeAssetAndSetDirty(undoMessage);
                ReinitNodesDependencies();
            }
        }

        private bool ProcessWatcherEvents()
        {
            if (BTModeManager.IsPlaymode)
            {
                if (_watcher.Current?.Tree != null && _watcher.Current.Tree != TreeInstance)
                {
                    InitializeWithAsset(_watcher.Current.Tree);
                    return true;
                }
            }
            else
            {
                if (TreeInstance == null || TreeInstance.IsRuntime)
                {
                    var asset = EditorUtility.InstanceIDToObject(_lastAssetID) as BehaviourTree;
                    InitializeWithAsset(asset);
                    return true;
                }
            }

            return false;
        }

        private void NodesMoved(HashSet<int> listIndices)
        {
            bool changed = _nodeProcessor.AfterNodesDragged(listIndices);
            UpdateTreeAssetAndSetDirty("Move Node");

            if (changed)
            {
                FetchGrapghFromAsset();
            }
        }

        private void DrawTopBar()
        {
            var rect = new Rect(Vector2.zero, new Vector2(position.width, TopBarHeight));
            EditorGUI.DrawRect(rect, s_topBarColor);
        }
    }
}
