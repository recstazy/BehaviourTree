using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNodeInspector : EditorWindow
    {
        public static event System.Action OnClosed;

        #region Fields

        private static readonly Vector2 s_nodeOffset = new Vector3(20f, -10f);
        private static readonly Vector2 s_defaultSize = new Vector2(255f, 300f);
        private static readonly Vector2 s_propertyBorder = new Vector2(5f, 5f);

        private BehaviourTree tree;
        private SerializedObject serializedObject;
        private SerializedProperty property;
        private static BTNodeInspector s_currentWindow;
        private BehaviourTreeNode currentNode;
        private Vector2 scrollPosition;
        
        #endregion

        #region Properties

        public static int CurrentNodeIndex { get; private set; } = -1;
        public static bool IsActive { get; private set; }

        #endregion

        private void OnDisable()
        {
            ApplyAndDisposeSerializedObject();
            currentNode = null;
            tree = null;
            property = null;
        }

        public static void SetupForSelection(BehaviourTreeNode currentSelection, BehaviourTreeWindow mainWindow)
        {
            if (currentSelection.Index == CurrentNodeIndex) return;
            if (CheckForNoEditor(currentSelection?.Data?.TaskImplementation)) return;

            var task = currentSelection.Data.TaskImplementation;

            if (task != null)
            {
                if (s_currentWindow != null)
                {
                    CloseInspector();
                }

                var window = GetWindow<BTNodeInspector>(true, ObjectNames.NicifyVariableName(task.GetType().Name), true);
                window.tree = mainWindow.TreeInstance;
                window.currentNode = currentSelection;
                window.position = new Rect(window.GetWindowPosition(mainWindow.position.position), s_defaultSize);
                s_currentWindow = window;
                CurrentNodeIndex = currentSelection.Index;
                IsActive = true;
            }
            else
            {
                if (s_currentWindow != null)
                {
                    CloseInspector();
                }
            }
        }

        public static void CloseInspector()
        {
            if (s_currentWindow?.tree != null)
            {
                OnClosed?.Invoke();
            }
            
            s_currentWindow?.Close();
            s_currentWindow = null;
            CurrentNodeIndex = -1;
            IsActive = false;
        }

        private static bool CheckForNoEditor(BehaviourTask task)
        {
            if (task == null) return true;
            if (task.GetType().GetCustomAttribute<NoInspectorAttribute>() != null) return true;

            return false;
        }

        private void OnGUI()
        {
            var property = CreateOrGetProperty();

            if (property != null)
            {
                Rect propertySafeArea = position;
                propertySafeArea.position = s_propertyBorder;
                propertySafeArea.size -= 2f * s_propertyBorder;

                property.isExpanded = true;
                float propHeight = EditorGUI.GetPropertyHeight(property);
                bool isBiggerThanWindow = propHeight > propertySafeArea.size.y;

                var propRect = new Rect(
                    propertySafeArea.position.x, 
                    propertySafeArea.position.y, 
                    isBiggerThanWindow ? propertySafeArea.width - 15f : propertySafeArea.width, 
                    propHeight);

                if (isBiggerThanWindow)
                {
                    var rect = position;
                    rect.position = s_propertyBorder;
                    rect.height -= s_propertyBorder.y * 2f;
                    rect.width -= 5f;
                    scrollPosition = GUI.BeginScrollView(rect, scrollPosition, propRect);
                }

                EditorGUI.PropertyField(propRect, property, true);

                if (isBiggerThanWindow)
                {
                    GUI.EndScrollView();
                }
            }

            ApplyAndDisposeSerializedObject();
        }

        private Vector2 GetWindowPosition(Vector2 mainWindowPosition)
        {
            var nodeRect = currentNode.GetTransformedRect();
            return (Vector2)GUI.matrix.MultiplyPoint3x4(nodeRect.position + Vector2.right * nodeRect.width) + mainWindowPosition + s_nodeOffset;
        }

        private SerializedProperty CreateOrGetProperty()
        {
            if (tree != null)
            {
                if (serializedObject == null || property == null)
                {
                    serializedObject = new SerializedObject(tree);
                    int dataIndex = -1;

                    for (int i = 0; i < tree.NodeData.Data.Length; i++)
                    {
                        if (tree.NodeData.Data[i].Index == currentNode.Index)
                        {
                            dataIndex = i;
                            break;
                        }
                    }

                    if (dataIndex >= 0)
                    {
                        var datasArray = serializedObject.FindProperty("_nodeData._data");
                        property = datasArray.GetArrayElementAtIndex(dataIndex).FindPropertyRelative("_taskImplementation");
                    }
                }

                return property;
            }

            return null;
        }

        private void ApplyAndDisposeSerializedObject()
        {
            serializedObject?.ApplyModifiedProperties();
            serializedObject?.Dispose();
            serializedObject = null;
        }
    }
}
