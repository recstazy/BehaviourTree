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
        public event Action<object> OnValueChanged;

        #region Fields

        private static StyleSheet PropertyStyles = 
            AssetDatabase.LoadAssetAtPath<StyleSheet>(System.IO.Path.Combine(MainPaths.UssRoot, "PropertyFieldElementStyles.uss"));

        private VisualElement _inputField;
        private PropertyFieldElement[] _subFields;
        private ListPropertyElement _listView;
        private bool _unwrap;
        private Label _label;

        private UnityEngine.Object _serializedTargetObject;
        private string _propertyPath;
        private string _displayName;
        private bool _isArrayAndNotString;

        #endregion

        #region Properties

        public string Label { get => _label.text; set => _label.text = value; }

        #endregion

        public PropertyFieldElement()
        {
            styleSheets.Add(PropertyStyles);
            RegisterCallback<DetachFromPanelEvent>(Detached);
        }

        public void SetProperty(SerializedProperty property, bool hideLabelAndUnwrap = false)
        {
            _serializedTargetObject = property.serializedObject.targetObject;
            _propertyPath = property.propertyPath;
            _displayName = property.displayName;
            _unwrap = hideLabelAndUnwrap;
            CreateInputField();
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            UnregisterCallback<DetachFromPanelEvent>(Detached);

            if (_subFields != null)
            {
                foreach (var s in _subFields)
                {
                    s.OnValueChanged -= SubfieldChanged;
                }
            }

            if (_listView != null)
            {
                _listView.OnChanged -= ListChanged;
                _listView.Close();
                _listView = null;
            }
        }

        private void CreateInputField()
        {
            if (_inputField != null)
            {
                Remove(_inputField);
                _inputField = null;
            }

            FieldUtility.CreateSerializedObjectAndProperty(_serializedTargetObject, _propertyPath, out var sObject, out var property);

            using (sObject)
            {
                using (property)
                {
                    _isArrayAndNotString = property.isArray && property.propertyType != SerializedPropertyType.String;
                    _label = new Label(_displayName);
                    bool isComplex = FieldUtility.IsComplex(property.propertyType);

                    if (!isComplex) CreateSimpleView(property);
                    else CreateComplexView(property);
                }
            }
        }

        private void ValueChanged(object oldValue, object newValue)
        {
            ApplyChangesToTarget(newValue);
            OnValueChanged?.Invoke(newValue);
        }

        private void SubfieldChanged(object newValue)
        {
            OnValueChanged?.Invoke(GetValue());
        }

        private void ApplyChangesToTarget(object newValue)
        {
            if (_isArrayAndNotString) return;
            SetValue(newValue);
        }

        private void CreateSimpleView(SerializedProperty property)
        {
            _inputField = FieldUtility.GetFieldByType(property, GetValue(), ValueChanged);
            _inputField.AddToClassList("simple-prop-value");
            var simpleContainer = new VisualElement();
            simpleContainer.AddToClassList("simple-prop-container");
            Add(simpleContainer);
            simpleContainer.Add(_label);
            simpleContainer.Add(_inputField);
        }

        private void CreateComplexView(SerializedProperty property)
        {
            VisualElement container;

            if (_unwrap)
            {
                container = this;
            }
            else
            {
                Add(_label);
                container = new VisualElement();
                container.AddToClassList("complex-prop-container");
                Add(container);
            }

            if (_isArrayAndNotString) CreateListView(property, container);
            else CreateSubfieldsView(property, container);
        }

        private void CreateSubfieldsView(SerializedProperty serializedProperty, VisualElement container)
        {
            var value = GetValue();

            if (value == null)
            {
                container.Add(FieldUtility.NoneLabel);
                return;
            }

            var subInfos = value.GetType().GetSerializedFieldsUpToBase();
            _subFields = new PropertyFieldElement[subInfos.Length];

            for (int i = 0; i < subInfos.Length; i++)
            {
                var subProp = subInfos[i];
                var property = serializedProperty.FindPropertyRelative(subProp.Name);
                var subField = new PropertyFieldElement();
                subField.SetProperty(property);
                _subFields[i] = subField;
                container.Add(subField);
                subField.OnValueChanged += SubfieldChanged;
            }
        }

        private void CreateListView(SerializedProperty serializedProperty, VisualElement container)
        {
            var listElement = new ListPropertyElement();
            listElement.SetProperty(serializedProperty);
            container.Add(listElement);
            _listView = listElement;
            _label.text = $"{_displayName} [{_listView.ArraySize}]";
            _listView.OnChanged += ListChanged;
        }

        private void ListChanged()
        {
            _label.text = $"{_displayName} [{_listView.ArraySize}]";
            OnValueChanged?.Invoke(null);
        }

        private object GetValue()
        {
            return FieldUtility.GetValue(_serializedTargetObject, _propertyPath);
        }

        private void SetValue(object value)
        {
            FieldUtility.SetValue(_serializedTargetObject, _propertyPath, value);
        }
    }
}
