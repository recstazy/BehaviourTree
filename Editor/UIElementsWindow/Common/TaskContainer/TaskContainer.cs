using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class TaskContainer : VisualElement
    {
        #region Fields

        private NodeData _data;
        private SerializedObject _serializedObject;
        private SerializedProperty _property;
        private PropertyField _field;

        #endregion

        #region Properties

        private BehaviourTree Tree => BTWindow.SharedTree;

        #endregion

        public void SetData(NodeData data)
        {
            _data = data;
            UpdateField();
        }

        public void Dispose()
        {
            _field?.Unbind();
            _serializedObject = null;
            _property = null;
            _data = null;
        }

        private void UpdateField()
        {
            _field?.Unbind();
            _serializedObject = null;
            Clear();
            bool hasEditor = !CheckForNoEditor(_data.TaskImplementation);
            if (hasEditor) CreatePropertyField();

            SetEnabled(hasEditor);
        }

        private static bool CheckForNoEditor(BehaviourTask task)
        {
            if (task == null) return true;
            if (task.GetType().GetCustomAttribute<NoInspectorAttribute>() != null) return true;

            return false;
        }

        private void CreatePropertyField()
        {
            var property = CreateOrGetTaskProperty();
            if (property == null) return;

            property = property.Copy();
            _field = new PropertyField(property);
            Add(_field);
            this.Bind(_serializedObject);
        }

        private SerializedProperty CreateOrGetTaskProperty()
        {
            if (Tree != null)
            {
                if (_serializedObject == null || _property == null)
                {
                    _serializedObject = new SerializedObject(Tree);
                    int dataIndex = -1;

                    for (int i = 0; i < Tree.NodeData.Data.Length; i++)
                    {
                        if (Tree.NodeData.Data[i].Index == _data.Index)
                        {
                            dataIndex = i;
                            break;
                        }
                    }

                    if (dataIndex >= 0)
                    {
                        var datasArray = _serializedObject.FindProperty("_nodeData._data");
                        _property = datasArray.GetArrayElementAtIndex(dataIndex).FindPropertyRelative("_taskImplementation");
                    }
                }

                return _property;
            }

            return null;
        }
    }
}
