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

        private SerializedProperty _property;
        private VisualElement _inputField;
        private PropertyFieldElement[] _subFields;
        private ListPropertyElement _listView;
        private bool _unwrap;
        private Label _label;

        private Func<object> _valueGetter;
        private Action<object> _valueSetter;

        
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
            Func<object> getter;
            Action<object> setter;
            FieldUtility.TryGetGetterAndSetter(property, out getter, out setter);
            SetProperty(property, getter, setter, hideLabelAndUnwrap);
        }

        private void SetProperty(SerializedProperty property, Func<object> valueGetter, Action<object> valueSetter, bool hideLabelAndUnwrap = false)
        {
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;
            _unwrap = hideLabelAndUnwrap;
            _property = property;
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

            if (_property != null) _property.Dispose();
        }

        private void CreateInputField()
        {
            if (_inputField != null)
            {
                Remove(_inputField);
                _inputField = null;
            }

            bool isComplex = FieldUtility.IsComplex(_property.propertyType);
            _label = new Label(_property.displayName);

            if (!isComplex) CreateSimpleView(_label);
            else CreateComplexView(_label);
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
            if (_property.isArray && _property.propertyType != SerializedPropertyType.String) return;
            SetValue(newValue);
            _property.serializedObject.ApplyModifiedProperties();
        }

        private void CreateSimpleView(Label label)
        {
            _inputField = FieldUtility.GetFieldByType(_property, GetValue(), ValueChanged);
            _inputField.AddToClassList("simple-prop-value");
            var simpleContainer = new VisualElement();
            simpleContainer.AddToClassList("simple-prop-container");
            Add(simpleContainer);
            simpleContainer.Add(label);
            simpleContainer.Add(_inputField);
        }

        private void CreateComplexView(Label label)
        {
            bool isArray = _property.isArray;
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

            if (isArray) CreateListView(container);
            else CreateSubfieldsView(container);
        }

        private void CreateSubfieldsView(VisualElement container)
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
                var property = _property.FindPropertyRelative(subProp.Name);
                var subField = new PropertyFieldElement();
                subField.SetProperty(property);
                _subFields[i] = subField;
                container.Add(subField);
                subField.OnValueChanged += SubfieldChanged;
            }
        }

        private void CreateListView(VisualElement container)
        {
            var listElement = new ListPropertyElement();
            listElement.SetProperty(_property);
            container.Add(listElement);
            _listView = listElement;
            _listView.OnChanged += ListChanged;
        }

        private void ListChanged()
        {
            OnValueChanged?.Invoke(null);
        }

        private object GetValue()
        {
            return _valueGetter?.Invoke();
        }

        private void SetValue(object value)
        {
            _valueSetter?.Invoke(value);
        }
    }
}
