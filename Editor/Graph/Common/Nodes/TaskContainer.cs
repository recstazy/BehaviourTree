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

        private TaskNodeData _data;
        private SerializedObject _serializedObject;
        private SerializedProperty _property;
        private PropertyFieldElement _field;
        private const string TaskImplName = "_taskImplementation";
        private string _currentClass;

        #endregion

        #region Properties

        private BehaviourTree Tree => BTWindow.SharedTree;

        #endregion

        public void SetData(TaskNodeData data)
        {
            _data = data;
            UpdateField();
        }

        public void Dispose()
        {
            if (_field != null) _field.OnChanged -= FieldChanged;
            if(_property != null) _property.Dispose();

            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedPropertiesWithoutUndo();
                _serializedObject.Dispose();
            }

            _serializedObject = null;
            _property = null;
            _data = null;
        }

        private void UpdateField()
        {
            _serializedObject = null;
            Clear();
            bool hasEditor = !CheckForNoEditor(_data.TaskImplementation);
            if (hasEditor) CreatePropertyField();
            AssignClass(hasEditor);
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
            _field = new PropertyFieldElement();
            _field.SetProperty(property, true);
            _field.OnChanged += FieldChanged;
            Add(_field);
        }

        private void FieldChanged()
        {
            BTWindow.SetDirty("Change task field");
        }

        private SerializedProperty CreateOrGetTaskProperty()
        {
            if (Tree != null)
            {
                if (_serializedObject == null || _property == null)
                {
                    _serializedObject = new SerializedObject(Tree);
                    int dataIndex = -1;

                    for (int i = 0; i < Tree.NodeData.TaskData.Length; i++)
                    {
                        if (Tree.NodeData.TaskData[i].Index == _data.Index)
                        {
                            dataIndex = i;
                            break;
                        }
                    }

                    if (dataIndex >= 0)
                    {
                        var datasArray = _serializedObject.FindProperty("_nodeData._taskData");
                        _property = datasArray.GetArrayElementAtIndex(dataIndex).FindPropertyRelative(TaskImplName);
                    }
                }

                return _property;
            }

            return null;
        }

        private void AssignClass(bool hasEditor)
        {
            string newClass = "task-container";
            if (!hasEditor) newClass += "-noeditor";

            if (_currentClass != newClass)
            {
                if (!string.IsNullOrEmpty(_currentClass))
                {
                    RemoveFromClassList(_currentClass);
                }
                
                AddToClassList(newClass);
            }

            _currentClass = newClass;
        }
    }
}
