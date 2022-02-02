using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BBWatcher : EditorWindow
    {
        #region Fields

        private VisualElement _bbContainer;
        private VisualElement _toolbar;
        private PlaymodeWatcher _playmodeWatcher;
        private TreeSelector _treeSelector;

        #endregion

        #region Properties

        #endregion

        [MenuItem("Window/Behaviour Tree/Blackboard Watcher")]
        private static void OpenWatcherWindow()
        {
            var window = GetWindow<BBWatcher>();
            window.titleContent = new GUIContent("Blackboard Watcher");
        }

        private void OnEnable()
        {
            TaskFactory.UpdateTaskTypes();
            ImportLayout();
            _playmodeWatcher.SetDependencies(_treeSelector);
            TreeSelector.OnTreeChanged += TreeSelectorTreeChanged;
        }

        private void OnDisable()
        {
            TreeSelector.OnTreeChanged -= TreeSelectorTreeChanged;
            VisualElement root = rootVisualElement;
            if (root.childCount > 0) root.Clear();
        }

        private void ImportLayout()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(System.IO.Path.Combine(MainPaths.UxmlRoot, "BBWatcherWindowLayout.uxml"));
            VisualElement windowLayout = visualTree.Instantiate();
            root.Add(windowLayout);
            windowLayout.StretchToParentSize();

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(System.IO.Path.Combine(MainPaths.UssRoot, "BTWindowStyles.uss"));
            root.styleSheets.Add(styleSheet);

            _bbContainer = rootVisualElement.Q(name: "bb-container");
            _toolbar = rootVisualElement.Q(className: "window-toolbar");
            _playmodeWatcher = _toolbar.Q<PlaymodeWatcher>();
            _treeSelector = _toolbar.Q<TreeSelector>();
        }

        private void TreeSelectorTreeChanged(BehaviourTree newTree)
        {
            if (_bbContainer.childCount > 0) _bbContainer.Clear();

            if (newTree != null && newTree.Blackboard != null)
            {
                var blackboard = newTree.Blackboard;
                var serializedObject = new SerializedObject(blackboard);

                var property = serializedObject.GetIterator();
                if (!property.Next(true)) return;
                property = property.Copy();

                //Skipping Script reference
                property.NextVisible(false);

                while (property.NextVisible(false))
                {
                    var field = new PropertyFieldElement();
                    field.SetProperty(property);
                    _bbContainer.Add(field);
                }
            }
        }
    }
}
