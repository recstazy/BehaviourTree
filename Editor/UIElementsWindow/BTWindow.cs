using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTWindow : EditorWindow
    {
        #region Fields

        private int _lastAssetID;

        private bool _hasTreeInitialized;
        private VisualElement _graphContainer;
        private VisualElement _toolbar;
        private BTGraph _graph;
        private PlaymodeWatcher _playmodeWatcher;
        private TargetTreeWatcher _treeWatcher;
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
                return true;
            }

            return false;
        }

        private static void ShowBtWindow(BehaviourTree asset)
        {
            BTWindow wnd = GetWindow<BTWindow>();
            s_currentWindow = wnd;
            wnd.Show();
            wnd.titleContent = new GUIContent(ObjectNames.NicifyVariableName(asset.name));
            wnd.InitializeWithAsset(asset);
        }

        internal static void SetDirty(string undoDescription = "")
        {
            bool canUndo = !string.IsNullOrEmpty(undoDescription);
            if (canUndo) Undo.RecordObject(SharedTree, undoDescription);
            EditorUtility.SetDirty(SharedTree);
        }

        internal static void UndoRedoPreformed()
        {
            s_currentWindow.InitializeWithAsset(SharedTree);
        }

        private void OnEnable()
        {
            TaskFactory.UpdateTaskTypes();
            ImportLayout();

            if (_lastAssetID != 0)
            {
                var assetObject = EditorUtility.InstanceIDToObject(_lastAssetID);

                if (assetObject is BehaviourTree treeAsset)
                {
                    if (SharedTree == null || SharedTree != treeAsset)
                    {
                        InitializeWithAsset(treeAsset);
                    }
                }
            }

            Undo.IncrementCurrentGroup();
        }

        private void OnDisable()
        {
            SetDirty();
            Undo.IncrementCurrentGroup();
            DisposeTree();
            VisualElement root = rootVisualElement;
            if (root.childCount > 0) root.Clear();
        }

        private void InitializeWithAsset(BehaviourTree asset)
        {
            if (_hasTreeInitialized) DisposeTree();
            SharedTree = asset;
            _lastAssetID = asset.GetInstanceID();
            
            InitializeGraph();
            _graph.OnStructureChanged += UpdatePlaymodeWatcher;
            UpdatePlaymodeWatcher();

            _hasTreeInitialized = true;
        }

        private void DisposeTree()
        {
            if (_graph != null)
            {
                _graph.OnStructureChanged -= UpdatePlaymodeWatcher;
                _graph.Dispose();
                _graphContainer.Clear();
                _graph = null;
            }
            
            SharedTree = null;
            s_currentWindow = null;
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
            _treeWatcher = _toolbar.Q<TargetTreeWatcher>();
        }

        private void InitializeGraph()
        {
            InitializeEntry();
            _graph = new BTGraph();
            _graph.Initialize(SharedTree);
            _graphContainer.Add(_graph);
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
            var edges = _graph.edges.ToList().Select(e => (IPlaymodeDependent)new EdgeReference(e)).ToArray();
            var nodes = _graph.BtNodes.Select(n => (IPlaymodeDependent)n).ToArray();

            _playmodeWatcher.SetDependencies(
                edges.Concat(nodes)
                .Concat(new IPlaymodeDependent[] { _graph, _treeWatcher })
                .ToArray());
        }
    }
}