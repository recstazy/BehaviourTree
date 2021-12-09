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
        private UndoHandler _undoHandler;
        
        private static BTWindow s_currentWindow;
        public static BehaviourTree SharedTree { get; private set; }
        public static BehaviourTree TreeInstance { get; private set; }

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
            wnd.titleContent = new GUIContent(asset.name);
            wnd.InitializeWithAsset(asset);
        }

        private void InitializeWithAsset(BehaviourTree asset)
        {
            SharedTree = asset;
            _lastAssetID = asset.GetInstanceID();
            TreeInstance = Instantiate(SharedTree);
            ImportLayout();
        }

        internal static void SetDirty(string undoDescription = "")
        {
            bool canUndo = !string.IsNullOrEmpty(undoDescription);

            if (canUndo)
            {
                Undo.RecordObject(SharedTree, undoDescription);
            }
            
            SharedTree.NodeData = new TreeNodeData(TreeInstance.NodeData.Data.ToArray());
            EditorUtility.SetDirty(SharedTree);
        }

        internal static void UndoRedoPreformed()
        {
            s_currentWindow.InitializeWithAsset(SharedTree);
        }

        private void OnGUI()
        {
            _undoHandler.OnGUI();
        }

        private void OnEnable()
        {
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

            _undoHandler = new UndoHandler();
            Undo.IncrementCurrentGroup();
        }

        private void OnDisable()
        {
            Undo.IncrementCurrentGroup();
            SetDirty();
            SharedTree = null;
            TreeInstance = null;
            s_currentWindow = null;
            _undoHandler = null;
        }

        private void ImportLayout()
        {
            VisualElement root = rootVisualElement;

            if (root.childCount > 0) root.Clear();

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(MainPaths.UxmlRoot, "BTWindowLayout.uxml"));
            VisualElement windowLayout = visualTree.Instantiate();
            root.Add(windowLayout);

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(MainPaths.UssRoot, "BTWindowStyles.uss"));
            root.styleSheets.Add(styleSheet);
        }
    }
}