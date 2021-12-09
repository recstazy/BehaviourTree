using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTWindow : EditorWindow
    {
        #region Fields

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
            wnd.Show();
            wnd.titleContent = new GUIContent(asset.name);
            wnd.InitializeWithAsset(asset);
        }

        private void InitializeWithAsset(BehaviourTree asset)
        {
            SharedTree = asset;
            TreeInstance = Instantiate(SharedTree);
            ImportLayout();
        }

        private void OnDisable()
        {
            SharedTree = null;
            TreeInstance = null;
        }

        private void ImportLayout()
        {
            VisualElement root = rootVisualElement;

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