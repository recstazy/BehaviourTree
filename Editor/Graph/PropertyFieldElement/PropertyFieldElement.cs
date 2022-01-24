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
    public class PropertyFieldElement : BasePropertyFieldElement
    {
        #region Fields

        private Func<int> _getArraySize;

        #endregion

        #region Properties

        /// <summary> Use only before SetProperty call </summary>
        public bool HideUnsupported { get; set; }
        public int SubfieldsCount => FieldsContainer.childCount;

        #endregion

        protected static Type GetCustomElementTypeForProperty(SerializedProperty property)
        {
            PropertyValueHelper.GetTargetObjectOfProperty(property, out FieldInfo fieldInfo);
            var fieldType = PropertyValueHelper.GetFieldType(fieldInfo);

            if (fieldType != null)
            {
                var elementTypes = TypeCache.GetTypesDerivedFrom<BasePropertyFieldElement>().ToArray();

                foreach (var t in elementTypes)
                {
                    var attribute = t.GetCustomAttribute<CustomPropertyElementAttribute>();

                    if (attribute != null && attribute.PropertyType.IsAssignableFrom(fieldType))
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        protected override void CreateVisualElements(SerializedProperty property)
        {
            if (FieldsContainer.childCount > 0) FieldsContainer.Clear();
            var customPropertyType = IsArrayAndNotString ? null : GetCustomElementTypeForProperty(property);

            if (customPropertyType != null)
            {
                var instance = Activator.CreateInstance(customPropertyType) as BasePropertyFieldElement;
                instance.SetProperty(property, true);
                AddSubfield(instance);
            }
            else
            {
                if (!IsComplex) CreateSimpleView(property);
                else CreateComplexView(property);
            }
        }

        protected override void Detached(DetachFromPanelEvent evt)
        {
            base.Detached(evt);
            _getArraySize = null;
        }

        private void SimpleValueChanged(object oldValue, object newValue)
        {
            if (!IsArrayAndNotString) SetTargetObjectValue(newValue);
            CallChanged();
        }

        protected override void SubfieldChanged()
        {
            if (IsArrayAndNotString)
            {
                LabelText = $"{_displayName} [{_getArraySize?.Invoke()}]";
            }
        }

        private void CreateSimpleView(SerializedProperty property)
        {
            var simpleField = FieldUtility.GetFieldByType(property, GetTargetObjectValue(), SimpleValueChanged);
            simpleField.AddToClassList("simple-prop-value");
            var simpleContainer = new VisualElement();
            simpleContainer.AddToClassList("simple-prop-container");
            simpleContainer.Add(Label);
            simpleContainer.Add(simpleField);
            FieldsContainer.Add(simpleContainer);
        }

        private void CreateComplexView(SerializedProperty property)
        {
            if (IsArrayAndNotString) CreateListView(property);
            else CreateSubfieldsView(property);
        }

        private void CreateSubfieldsView(SerializedProperty serializedProperty)
        {
            var value = GetTargetObjectValue();

            if (value == null)
            {
                FieldsContainer.Add(FieldUtility.NoneLabel);
                return;
            }

            var subInfos = value.GetType().GetSerializedFieldsUpToBase();

            for (int i = 0; i < subInfos.Length; i++)
            {
                var subProp = subInfos[i];
                var property = serializedProperty.FindPropertyRelative(subProp.Name);
                var subField = new PropertyFieldElement();
                subField.HideUnsupported = HideUnsupported;
                subField.SetProperty(property);
                if (HideUnsupported && subField.SubfieldsCount == 0) continue;

                AddSubfield(subField);
            }
        }

        private void CreateListView(SerializedProperty serializedProperty)
        {
            var listElement = new ListPropertyElement();
            listElement.SetProperty(serializedProperty);
            AddSubfield(listElement);
            LabelText = $"{_displayName} [{listElement.ArraySize}]";
            _getArraySize = () => listElement.ArraySize;
        }
    }
}
