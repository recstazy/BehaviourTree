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

        private int lastAssetID;
        private BehaviourTree treeInstance;
        private BehaviourTree treeAsset;
        private BTEventProcessor eventProcessor;
        private NodeConnector connector;
        private BTModeManager modeManager;
        private BTTargetWatcher watcher;
        private BTNodeProcessor nodeProcessor;
        private bool initialized;

        #endregion

        #region Properties

        public static Vector2 GraphPosition => BTEventProcessor.CurrentGraphPosition;
        public BehaviourTree TreeInstance => treeInstance;
        public BehaviourTree TreeAsset => treeAsset;

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
            if (lastAssetID != 0)
            {
                var assetObject = EditorUtility.InstanceIDToObject(lastAssetID);

                if (assetObject is BehaviourTree treeAsset)
                {
                    if (treeInstance is null || this.treeAsset != treeAsset)
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
            if (initialized)
            {
                watcher.OnGUI(position);

                if (ProcessWatcherEvents())
                {
                    return;
                }

                eventProcessor.BeginZoom();
                connector.OnGUI();
                ProcessConnectorShedules();
                nodeProcessor.OnNodesGUI();

                eventProcessor.CanSelect = !connector.IsPerformingConnection;
                eventProcessor.OnGUI();
                eventProcessor.Process(Event.current);

                if (eventProcessor.DeleteNodesPressed)
                {
                    if (nodeProcessor.DeleteNodesBySelection(eventProcessor.Selection))
                    {
                        UpdateTreeAssetAndSetDirty("Delete Nodes");
                        nodeProcessor.ClearNodesDirty();
                        ReinitNodesDependencies();
                    }
                }
                else
                {
                    SetAssetDirtyIfNodesDirty();
                }

                eventProcessor.EndZoom();
                modeManager.OnGUI(position);
            }
        }

        private void EditorUpdate()
        {
            if (initialized)
            {
                if (BTModeManager.IsPlaymode)
                {
                    if (nodeProcessor.UpdateRunningNodesAndCheckChanged())
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
            if (treeInstance != null) Dispose();
            if (tree is null) return;
            if (!tree.IsRuntime) lastAssetID = tree.GetInstanceID();

            treeAsset = tree;
            Title = treeAsset.name;
            InitializeTreeAssetIfEmpty();

            BTNodeInspector.OnClosed += UpdateAssetAfterNodeInspector;
            TaskFactory.UpdateTaskTypes();
            NodeDrawerProvider.UpdateTaskDrawers();
            modeManager = new BTModeManager();
            watcher = new BTTargetWatcher();
            nodeProcessor = new BTNodeProcessor();
            FetchGrapghFromAsset();
            initialized = true;
        }

        private void InitializeTreeAssetIfEmpty()
        {
            if (treeAsset.EntryNode is null)
            {
                treeAsset.CreateEntry();
                EditorUtility.SetDirty(treeAsset);
            }
        }

        private void FetchGrapghFromAsset()
        {
            treeInstance = treeAsset.IsRuntime ? treeAsset : Instantiate(treeAsset);
            nodeProcessor.RecreateNodes(treeInstance);
            ReinitNodesDependencies();
        }

        private void Dispose()
        {
            SetDirtyAndSaveSaveAll();
            treeInstance = null;
            treeAsset = null;
            nodeProcessor?.Dispose();
            nodeProcessor = null;
            eventProcessor?.Dispose();
            eventProcessor = null;
            connector?.Dispose();
            connector = null;
            modeManager?.Dispose();
            modeManager = null;
            watcher?.Dispose();
            watcher = null;
            BTNodeInspector.OnClosed -= UpdateAssetAfterNodeInspector;
            BTNodeInspector.CloseInspector();
            initialized = false;
        }

        private void SetDirtyAndSaveSaveAll()
        {
            UpdateTreeAssetAndSetDirty();
            SaveAssets();
        }

        private void SaveAssets()
        {
            if (!treeInstance.IsRuntime)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void UpdateAssetAfterNodeInspector()
        {
            if (treeAsset != null && treeInstance != null)
            {
                UpdateTreeAssetAndSetDirty("Edit Node Task");
            }
        }

        private void UpdateTreeAssetAndSetDirty(string undoName)
        {
            Undo.RecordObject(treeAsset, undoName);
            UpdateTreeAssetAndSetDirty();
        }

        private void UpdateTreeAssetAndSetDirty()
        {
            treeAsset.NodeData = new TreeNodeData(nodeProcessor.Nodes.Select(n => n.Data).ToArray());
            treeAsset.GraphPosition = BTEventProcessor.CurrentGraphPosition;
            treeAsset.Zoom = EditorZoomer.Zoom;

            if (!treeInstance.IsRuntime)
            {
                EditorUtility.SetDirty(treeAsset);
            }
        }

        private void CreateNewNodeInEditor()
        {
            nodeProcessor.CreateNewNodeInEditor();
            UpdateTreeAssetAndSetDirty("Add Node");
            FetchGrapghFromAsset();
        }

        private void SetAssetDirtyIfNodesDirty()
        {
            if (nodeProcessor.NodesDirty)
            {
                nodeProcessor.ClearNodesDirty();
                UpdateTreeAssetAndSetDirty("Change Node Task");
                FetchGrapghFromAsset();
            }
        }

        private void ReinitNodesDependencies()
        {
            connector?.Dispose();
            connector = new NodeConnector(nodeProcessor.Nodes);

            eventProcessor?.Dispose();
            eventProcessor = new BTEventProcessor(this, nodeProcessor.Nodes);
            BTEventProcessor.CurrentGraphPosition = treeAsset.GraphPosition;
            EditorZoomer.Zoom = treeAsset.Zoom;
            eventProcessor.OnUndo = FetchGrapghFromAsset;
            eventProcessor.OnSetDirty = UpdateTreeAssetAndSetDirty;
            eventProcessor.OnRegisterUndoAndSetDirty = UpdateTreeAssetAndSetDirty;
            eventProcessor.OnCreateNode = CreateNewNodeInEditor;
            eventProcessor.OnCopy = nodeProcessor.CopySelectedToClipboard;
            eventProcessor.OnPaste = nodeProcessor.CreateNewNodesFromClipboardUnderMouse;
            eventProcessor.OnDuplicate = nodeProcessor.DuplicateNodes;
            eventProcessor.OnUpdateNodes = FetchGrapghFromAsset;
            eventProcessor.OnNodesMoved = NodesMoved;

            GUI.changed = true;
            Repaint();
        }

        private void ProcessConnectorShedules()
        {
            bool changed = connector.IsRemovalPending || connector.IsConnectionPending;
            string undoMessage = null;

            if (changed)
            {
                if (connector.IsConnectionPending)
                {
                    undoMessage = "Create Connection";
                }
                else
                {
                    undoMessage = "Remove Connection";
                }
            }

            if (connector.IsRemovalPending)
            {
                nodeProcessor.RemoveConnection(connector.RemovalPending);
            }

            if (connector.IsConnectionPending)
            {
                nodeProcessor.CreateConnection(connector.ConnectionPending);
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
                if (watcher.Current?.Tree != null && watcher.Current.Tree != treeInstance)
                {
                    InitializeWithAsset(watcher.Current.Tree);
                    return true;
                }
            }
            else
            {
                if (treeInstance == null || treeInstance.IsRuntime)
                {
                    var asset = EditorUtility.InstanceIDToObject(lastAssetID) as BehaviourTree;
                    InitializeWithAsset(asset);
                    return true;
                }
            }

            return false;
        }

        private void NodesMoved(HashSet<int> listIndices)
        {
            bool changed = nodeProcessor.AfterNodesDragged(listIndices);
            UpdateTreeAssetAndSetDirty("Move Node");

            if (changed)
            {
                FetchGrapghFromAsset();
            }
        }
    }
}
