using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTWindow : EditorWindow, IPlaymodeDependent
    {
        #region Fields

        private int _lastAssetId;
        private int _lastEditedId;

        private bool _hasTreeInitialized;
        private VisualElement _graphContainer;
        private VisualElement _toolbar;
        private BTGraph _graph;
        private PlaymodeWatcher _playmodeWatcher;
        private TreeSelector _treeSelector;
        private PlaymodeNodeHighlighter _nodeHighlighter;

        private static BTWindow s_currentWindow;
        public static BehaviourTree SharedTree { get; private set; }

        #endregion

        #region Properties

        #endregion

        [MenuItem("Window/Behaviour Tree/Force Close Window")]
        private static void ForceCloseWindow()
        {
            try
            {
                BTWindow wnd = GetWindow<BTWindow>();
                s_currentWindow = null;
                wnd.Close();
            }
            catch { }
        }

        [OnOpenAsset(2)]
        private static bool AssetOpened(int instanceID, int line)
        {
            var assetObject = EditorUtility.InstanceIDToObject(instanceID);

            if (assetObject is BehaviourTree treeAsset)
            {
                ShowBtWindow(treeAsset);
                s_currentWindow._lastEditedId = instanceID;
                return true;
            }

            return false;
        }

        private static void ShowBtWindow(BehaviourTree asset)
        {
            if (s_currentWindow == null)
            {
                s_currentWindow = GetWindow<BTWindow>();
                s_currentWindow.Show();
            }
            else s_currentWindow.Focus();

            var wnd = s_currentWindow;
            wnd.titleContent = new GUIContent(ObjectNames.NicifyVariableName(asset.name));
            wnd.InitializeWithAsset(asset);
        }

        private static void UndoRedoPreformed()
        {
            if (SharedTree == null) return;

            if (BTUndo.ApplyUndoRedo(SharedTree))
            {
                EditorUtility.SetDirty(SharedTree);
                s_currentWindow.InitializeWithAsset(SharedTree);
            }
        }

        internal static void SetDirty(string undoDescription = "")
        {
            bool canUndo = !string.IsNullOrEmpty(undoDescription);
            if (canUndo) BTUndo.RegisterUndo(SharedTree, undoDescription);
            EditorUtility.SetDirty(SharedTree);
        }

        private void OnEnable()
        {
            s_currentWindow = this;
            TaskFactory.UpdateTaskTypes();
            ImportLayout();
            _nodeHighlighter = new PlaymodeNodeHighlighter();
            _nodeHighlighter.Bind(() => _graph.BtNodes);
            TreeSelector.OnTreeChanged += TreeSelectorTreeChanged;

            if (_lastAssetId != 0)
            {
                var assetObject = EditorUtility.InstanceIDToObject(_lastAssetId);

                if (assetObject is BehaviourTree treeAsset)
                {
                    if (SharedTree == null || SharedTree != treeAsset)
                    {
                        InitializeWithAsset(treeAsset);
                    }
                }
            }

            Undo.undoRedoPerformed += UndoRedoPreformed;
        }

        private void OnDisable()
        {
            TreeSelector.OnTreeChanged -= TreeSelectorTreeChanged;
            SetDirty();
            Undo.undoRedoPerformed -= UndoRedoPreformed;
            DisposeTree();
            VisualElement root = rootVisualElement;
            if (root.childCount > 0) root.Clear();
            s_currentWindow = null;
        }

        public void PlaymodeChanged(bool isPlaymode)
        {
            if (!isPlaymode)
            {
                if (_lastEditedId != 0 && _lastEditedId != _lastAssetId)
                {
                    var newTree = (BehaviourTree)EditorUtility.InstanceIDToObject(_lastEditedId);
                    InitializeWithAsset(newTree);
                }
            }
        }

        private void ExecuteCommand(ExecuteCommandEvent evt)
        {
            if (evt.commandName == "UndoRedoPerformed")
            {
                evt.StopImmediatePropagation();
                evt.imguiEvent.Use();
                UndoRedoPreformed();
            }
        }

        private void InitializeWithAsset(BehaviourTree asset)
        {
            if (_hasTreeInitialized) DisposeTree();
            SharedTree = asset;
            _lastAssetId = asset.GetInstanceID();

            InitializeGraph();
            _hasTreeInitialized = true;
        }

        private void DisposeTree()
        {
            if (_graph != null)
            {
                _graph.OnStructureChanged -= UpdatePlaymodeWatcher;
                _graph.Dispose();
                _graph.SetEnabled(false);
                _graph.Unbind();
                _graph.RemoveFromHierarchy();
                _graphContainer.Clear();
                _graph = null;
            }

            _nodeHighlighter.Clear();
            SharedTree = null;
            
            _hasTreeInitialized = false;
        }

        private void ImportLayout()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(MainPaths.UxmlRoot, "BTWindowLayout.uxml"));
            VisualElement windowLayout = visualTree.Instantiate();
            root.Add(windowLayout);
            windowLayout.StretchToParentSize();

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(MainPaths.UssRoot, "BTWindowStyles.uss"));
            root.styleSheets.Add(styleSheet);

            _graphContainer = rootVisualElement.Q(name: "graph-container");
            _toolbar = rootVisualElement.Q(className: "window-toolbar");
            _playmodeWatcher = _toolbar.Q<PlaymodeWatcher>();
            _treeSelector = _toolbar.Q<TreeSelector>();
        }

        private void InitializeGraph()
        {
            InitializeEntry();
            AddGraphDelayed();
        }

        // Seems GraphView needs some time to detach from parent 
        // and if we instantly add new graph after deleting old one it doesn't detach
        private void AddGraphDelayed()
        {
            rootVisualElement.SetEnabled(false);

            // Using Animation here as coroutine with callback
            var anim = UnityEngine.UIElements.Experimental
                .ValueAnimation<bool>.Create(rootVisualElement, (a, b, t) => false);
            
            anim.OnCompleted(() =>
            {
                AddGraphImmediate();
                rootVisualElement.SetEnabled(true);
            });

            anim.durationMs = 1;
            anim.Start();
        }

        private void AddGraphImmediate()
        {
            _graph = new BTGraph();
            _graph.Initialize(SharedTree);
            _graphContainer.Add(_graph);
            _graph.OnStructureChanged += UpdatePlaymodeWatcher;
            UpdatePlaymodeWatcher();
        }

        private void InitializeEntry()
        {
            if (SharedTree.EntryNode?.TaskImplementation == null)
            {
                SharedTree.CreateEntry();
                EditorUtility.SetDirty(SharedTree);
            }
        }

        private void UpdatePlaymodeWatcher()
        {
            var edges = _graph.Edges.ToArray();
            var nodes = _graph.BtNodes.Select(n => (IPlaymodeDependent)n).ToArray();

            _playmodeWatcher.SetDependencies(
                edges.Concat(nodes)
                .Concat(new IPlaymodeDependent[] { this, _graph, _treeSelector, _nodeHighlighter })
                .ToArray());
        }

        private void TreeSelectorTreeChanged(BehaviourTree newTree)
        {
            DisposeTree();

            if (newTree == null)
            {
                newTree = (BehaviourTree)EditorUtility.InstanceIDToObject(_lastEditedId);
            }

            InitializeWithAsset(newTree);
        }
    }
}