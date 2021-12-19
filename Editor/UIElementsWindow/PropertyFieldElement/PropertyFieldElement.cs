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
    public class PropertyFieldElement : VisualElement
    {
        #region Fields

        private FieldInfo _fieldInfo;
        private SerializedProperty _property;
        private VisualElement _inputField;
        private object _target;

        private PropertyFieldElement[] _subFields;
        private bool _unwrap;

        #endregion

        #region Properties
	
        #endregion

        public void SetField(SerializedProperty property, FieldInfo fieldInfo, object target, bool hideLabelAndUnwrap = false)
        {
            _unwrap = hideLabelAndUnwrap;
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
            var label = new Label(_property.displayName);

            if (!isComplex)
            {
                _inputField = FieldUtility.GetFieldByType(_property, ValueChanged);
                _inputField.AddToClassList("simple-prop-value");
                var simpleContainer = new VisualElement();
                simpleContainer.AddToClassList("simple-prop-container");
                Add(simpleContainer);
                simpleContainer.Add(label);
                simpleContainer.Add(_inputField);
            }
            else
            {
                VisualElement container;

                if (_unwrap)
                {
                    container = this;
                }
                else
                {
                    Add(label);
                    container = new VisualElement();
                    container.AddToClassList("complex-prop-container");
                    Add(container);
                }
                
                var subTarget = _fieldInfo.GetValue(_target);
                var targetType = subTarget.GetType();
                var subProps = targetType.GetSerializedFieldsUpToBase();
                _subFields = new PropertyFieldElement[subProps.Length];

                for (int i = 0; i < subProps.Length; i++)
                {
                    var subProp = subProps[i];
                    var property = _property.FindPropertyRelative(subProp.Name);
                    var subField = new PropertyFieldElement();
                    subField.SetField(property, subProp, subTarget);
                    _subFields[i] = subField;
                    container.Add(subField);
                }
            }
        }

        private void ValueChanged(object oldValue, object newValue)
        {
            Debug.Log($"Value changed: {oldValue} -> {newValue}");
        }
    }
}
