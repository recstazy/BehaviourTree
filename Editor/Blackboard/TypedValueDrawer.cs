using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyDrawer(typeof(ITypedValue), true)]
    internal class TypedValueDrawer : PropertyDrawer
    {
        #region Fields

        private const float RowHeight = 20f;
        private const float ChildLeftPadding = 0f;

        private Rect rect;
        private SerializedProperty property;
        private ValueTypeProvider typeProvider;
        private GUIContent label;

        #endregion

        #region Properties

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = RowHeight + GetChildHeight(property);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            rect = position;
            this.property = property;
            this.label = label;

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                DrawType();
                DrawValue();
            }
            else
            {
                EditorGUI.PropertyField(position, property, true);
            }
        }

        private void DrawType()
        {
            if (typeProvider is null)
            {
                typeProvider = new ValueTypeProvider(property.managedReferenceFullTypename);
            }

            var currentRect = rect;
            currentRect.height = RowHeight;
            typeProvider.OnGUI(currentRect, label.text);

            if (typeProvider.Changed)
            {
                ITypedValue newValue = null;

                if (typeProvider.CurrentType != null)
                {
                    newValue = Activator.CreateInstance(typeProvider.CurrentType) as ITypedValue;
                }

                typeProvider = null;
                property.managedReferenceValue = newValue;
            }
        }

        private void DrawValue()
        {
            var currentRect = rect;
            currentRect.y += RowHeight;
            currentRect.height = RowHeight;
            currentRect.x += ChildLeftPadding;
            currentRect.width -= ChildLeftPadding;

            var propCopy = property.Copy();
            var nextPropInThisParent = property.Copy();
            nextPropInThisParent.Next(false);

            if (propCopy.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(nextPropInThisParent, propCopy)) break;

                    EditorGUI.PropertyField(currentRect, propCopy, true);
                    currentRect.y += EditorGUI.GetPropertyHeight(propCopy, true);
                }
                while (propCopy.Next(false));
            }
        }

        private float GetChildHeight(SerializedProperty property)
        {
            var propCopy = property.Copy();
            var nextPropInThisParent = property.Copy();
            nextPropInThisParent.Next(false);
            float height = 0f;

            if (propCopy.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(nextPropInThisParent, propCopy)) break;
                    height += EditorGUI.GetPropertyHeight(propCopy);
                }
                while (propCopy.Next(false));
            }

            return height;
        }
    }
}
