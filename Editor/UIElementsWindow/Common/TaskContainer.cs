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
        private List<PropertyField> _fields;

        #endregion

        #region Properties

        private BehaviourTree Tree => BTWindow.SharedTree;

        #endregion

        public void SetData(NodeData data)
        {
            _data = data;
            UpdateFields();
        }

        private void UpdateFields()
        {
            UnbindProperties();
            Clear();
            bool hasEditor = !CheckForNoEditor(_data.TaskImplementation);
            if (hasEditor) CreateProperties();

            SetEnabled(hasEditor);
        }

        private static bool CheckForNoEditor(BehaviourTask task)
        {
            if (task == null) return true;
            if (task.GetType().GetCustomAttribute<NoInspectorAttribute>() != null) return true;

            return false;
        }

        private void CreateProperties()
        {
            _fields = new List<PropertyField>();
            var property = CreateOrGetTaskProperty();
            if (property == null) return;
            property = property.Copy();

            if (property.NextVisible(true))
            {
                int depth = property.depth;

                do
                {
                    var field = new PropertyField(property);
                    _fields.Add(field);
                    Add(field);
                }
                while (property.NextVisible(false) && depth == property.depth);
            }

            this.Bind(_serializedObject);
        }

        private void UnbindProperties()
        {
            if (_fields != null)
            {
                foreach (var f in _fields)
                {
                    f.Unbind();
                }
            }

            _serializedObject = null;
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
                        if (Tree.NodeData.Data[i].Guid == _data.Guid)
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
