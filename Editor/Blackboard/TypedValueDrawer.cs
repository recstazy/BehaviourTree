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

        private Rect _rect;
        private SerializedProperty _property;
        private ValueTypeProvider _typeProvider;
        private bool _isReadonly = false;
        private Type _forcedType;

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
            _rect = position;
            _property = property;

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

        internal void ForceSetType(Type forceType, bool isReadonly) 
        {
            if (forceType != null && typeof(ITypedValue).IsAssignableFrom(forceType))
            {
                _forcedType = forceType;
                _isReadonly = isReadonly;
            }
        }

        private void DrawType()
        {
            EditorGUI.BeginDisabledGroup(_isReadonly);

            if (_typeProvider is null)
            {
                string typename = _property.managedReferenceFullTypename;

                if (_forcedType != null)
                {
                    typename = ValueTypeProvider.GetManagedTypeFullName(_forcedType);
                }

                _typeProvider = new ValueTypeProvider(typename);
            }

            var currentRect = _rect;
            currentRect.height = RowHeight;
            _typeProvider.OnGUI(currentRect);

            if (_typeProvider.Changed || (_forcedType != null && ValueTypeProvider.GetManagedTypeFullName(_forcedType) != _property.managedReferenceFullTypename))
            {
                ITypedValue newValue = null;

                if (_typeProvider.CurrentType != null)
                {
                    newValue = Activator.CreateInstance(_typeProvider.CurrentType) as ITypedValue;
                }

                _typeProvider = null;
                _property.managedReferenceValue = newValue;
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawValue()
        {
            var currentRect = _rect;
            currentRect.y += RowHeight;
            currentRect.height = RowHeight;
            currentRect.x += ChildLeftPadding;
            currentRect.width -= ChildLeftPadding;

            var propCopy = _property.Copy();
            var nextPropInThisParent = _property.Copy();
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
