using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Reflection;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class PropertyDrawerField : VisualElement
    {
        #region Fields

        private FieldInfo _fieldInfo;
        private SerializedProperty _property;
        private VisualElement _inputField;
        private object _target;

        private PropertyDrawerField[] _subFields;

        #endregion

        #region Properties
	
        #endregion

        public void SetField(SerializedProperty property, FieldInfo fieldInfo, object target)
        {
            _fieldInfo = fieldInfo;
            _property = property;
            _target = target;
            CreateInputField();
        }

        private void CreateInputField()
        {
            if (_inputField != null)
            {
                Remove(_inputField);
                _inputField = null;
            }

            bool isComplex = FieldUtility.IsComplex(_property.propertyType);

            if (!isComplex)
            {
                _inputField = FieldUtility.GetFieldByType(_property, ValueChanged);
                Add(_inputField);
            }
            else
            {
                var subTarget = _fieldInfo.GetValue(_target);
                var subProps = subTarget.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                subProps = subProps.Where(p => p.IsSerialized()).ToArray();
                _subFields = new PropertyDrawerField[subProps.Length];

                for (int i = 0; i < subProps.Length; i++)
                {
                    var subProp = subProps[i];
                    var property = _property.FindPropertyRelative(subProp.Name);
                    var subField = new PropertyDrawerField();
                    subField.SetField(property, subProp, subTarget);
                    _subFields[i] = subField;
                    Add(subField);
                }
            }
        }

        private void ValueChanged(object newValue)
        {
            Debug.Log($"New Value: {newValue}");
        }

        
    }
}
