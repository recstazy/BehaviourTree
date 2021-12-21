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

        public void SetField(SerializedProperty property, FieldInfo fieldInfo, object target, bool hideLabelAndUnwrap = false)
        {
            SetField(property, () => fieldInfo.GetValue(target), (value) => fieldInfo.SetValue(target, value), hideLabelAndUnwrap);
        }

        public void SetField(SerializedProperty property, Func<object> valueGetter, Action<object> valueSetter, bool hideLabelAndUnwrap = false)
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
            var fieldValue = GetValue();

            if (!isComplex) CreateSimpleView(fieldValue, _label);
            else CreateComplexView(fieldValue, _label);
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
            SetValue(newValue);
        }

        private void CreateSimpleView(object fieldValue, Label label)
        {
            _inputField = FieldUtility.GetFieldByType(_property.propertyType, fieldValue, ValueChanged);
            _inputField.AddToClassList("simple-prop-value");
            var simpleContainer = new VisualElement();
            simpleContainer.AddToClassList("simple-prop-container");
            Add(simpleContainer);
            simpleContainer.Add(label);
            simpleContainer.Add(_inputField);
        }

        private void CreateComplexView(object fieldValue, Label label)
        {
            bool isArray = fieldValue != null && _property.isArray;
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

            if (isArray) CreateListView(fieldValue, container);
            else CreateSubfieldsView(fieldValue, container);
        }

        private void CreateSubfieldsView(object fieldValue, VisualElement container)
        {
            if (fieldValue == null)
            {
                container.Add(FieldUtility.NoneLabel);
                return;
            }

            var subTarget = fieldValue;
            var targetType = subTarget.GetType();
            var subInfos = targetType.GetSerializedFieldsUpToBase();
            _subFields = new PropertyFieldElement[subInfos.Length];

            for (int i = 0; i < subInfos.Length; i++)
            {
                var subProp = subInfos[i];
                var property = _property.FindPropertyRelative(subProp.Name);
                var subField = new PropertyFieldElement();
                subField.SetField(property, subProp, subTarget);
                _subFields[i] = subField;
                container.Add(subField);
                subField.OnValueChanged += SubfieldChanged;
            }
        }

        private void CreateListView(object fieldValue, VisualElement container)
        {
            var iList = fieldValue as IList;
            _listView = new ListPropertyElement();
            _listView.SetList(iList, _property);
            _listView.OnChanged += ListChanged;
            container.Add(_listView);
        }

        private void ListChanged(IList list)
        {
            ApplyChangesToTarget(list);
            SubfieldChanged(list);
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
