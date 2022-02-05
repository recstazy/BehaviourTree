using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BasePropertyFieldElement : VisualElement
    {
        public event System.Action OnChanged;

        #region Fields

        protected Object _serializedTargetObject;
        protected string _propertyPath;
        protected string _displayName;
        protected List<BasePropertyFieldElement> _subfields = new List<BasePropertyFieldElement>();

        private bool _unwrap;
        private static StyleSheet PropertyStyles =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(System.IO.Path.Combine(MainPaths.UssRoot, "PropertyFieldElementStyles.uss"));

        private FieldInfo _fieldInfo;

        #endregion

        #region Properties

        public string LabelText { get => Label.text; set => Label.text = value; }
        protected bool IsComplex { get; private set; }
        protected bool IsArrayAndNotString { get; private set; }
        protected Label Label { get; private set; }
        protected VisualElement FieldsContainer { get; private set; }
        protected FieldInfo FieldInfo => _fieldInfo;
        
        #endregion

        public BasePropertyFieldElement()
        {
            styleSheets.Add(PropertyStyles);
            RegisterCallback<DetachFromPanelEvent>(Detached);
        }

        public virtual void SetProperty(SerializedProperty property, bool hideLabelAndUnwrap = false)
        {
            _serializedTargetObject = property.serializedObject.targetObject;
            _propertyPath = property.propertyPath;
            _displayName = property.displayName;
            _unwrap = hideLabelAndUnwrap;
            PropertyValueHelper.GetTargetObjectOfProperty(property, out _fieldInfo);

            CreateField();
        }

        protected virtual void Detached(DetachFromPanelEvent evt)
        {
            UnregisterCallback<DetachFromPanelEvent>(Detached);

            if (_subfields != null)
            {
                foreach (var s in _subfields)
                {
                    s.OnChanged -= SubfieldChanged;
                }
            }
        }

        private void CreateField()
        {
            FieldUtility.CreateSerializedObjectAndProperty(_serializedTargetObject, _propertyPath, out var sObject, out var property);

            using (sObject)
            {
                using (property)
                {
                    Label = new Label(_displayName);
                    IsComplex = FieldUtility.IsComplex(property);
                    IsArrayAndNotString = property.isArray && property.propertyType != SerializedPropertyType.String;

                    if (_unwrap || !IsComplex) FieldsContainer = this;
                    else
                    {
                        Add(Label);
                        FieldsContainer = new VisualElement();
                        FieldsContainer.AddToClassList("complex-prop-container");
                        Add(FieldsContainer);
                    }

                    CreateVisualElements(property);
                }
            }
        }

        protected virtual void CreateVisualElements(SerializedProperty property) { }
        protected virtual void SubfieldChanged() { }

        protected void CallChanged()
        {
            OnChanged?.Invoke();
        }

        public object GetRelativeValue(string relativePath)
        {
            return FieldUtility.GetValue(_serializedTargetObject, $"{_propertyPath}.{relativePath}");
        }

        public object GetTargetObjectValue()
        {
            return FieldUtility.GetValue(_serializedTargetObject, _propertyPath);
        }

        public void SetTargetObjectValue(object value)
        {
            FieldUtility.SetValue(_serializedTargetObject, _propertyPath, value);
        }

        public void SetRelativeValue(string relativePath, object value)
        {
            FieldUtility.SetValue(_serializedTargetObject, $"{_propertyPath}.{relativePath}", value);
        }

        protected void AddSubfield(BasePropertyFieldElement field)
        {
            _subfields.Add(field);
            FieldsContainer.Add(field);
            field.OnChanged += SubfieldWasChanged;
        }

        private void SubfieldWasChanged()
        {
            SubfieldChanged();
            CallChanged();
        }
    }
}
