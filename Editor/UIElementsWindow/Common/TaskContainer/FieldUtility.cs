using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Reflection;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public static class FieldUtility
    {
        public static VisualElement ComplexLabel => new Label("Complex Field");
        public static VisualElement NotSupportedLabel => new Label("Not Supported");
        public static VisualElement NoneLabel => new Label("None");
        
        public static bool IsComplex(SerializedPropertyType type)
        {
            switch (type)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                    return true;
            }

            return false;
        }

        public static bool IsSerializedAndVisible(this FieldInfo info)
        {
            if (info.GetCustomAttribute<HideInInspector>() != null) return false;

            if (info.IsPublic) return true;
            else if (info.GetCustomAttribute<SerializeField>() != null) return true;
            else if (info.GetCustomAttribute<SerializeReference>() != null) return true;
            else return false;
        }

        public static bool IsGenericList(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public static VisualElement GetFieldByType(SerializedProperty property, object curValue, Action<object, object> onValueChanged)
        {
            if (IsComplex(property.propertyType)) return ComplexLabel;
            else return CreateFieldByPropertyType(property, curValue, onValueChanged);
        }

        public static FieldInfo[] GetSerializedFieldsUpToBase(this Type type)
        {
            List<Type> hierarchy = new List<Type>();
            var currentType = type;

            while (currentType != null)
            {
                hierarchy.Add(currentType);
                currentType = currentType.BaseType;
            }

            hierarchy.Reverse();
            List<FieldInfo> fields = new List<FieldInfo>();

            foreach (var t in hierarchy)
            {
                var subProps = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                subProps = subProps.Where(p => p.IsSerializedAndVisible()).ToArray();

                foreach (var s in subProps)
                {
                    fields.Add(s);
                }
            }

            return fields.ToArray();
        }

        public static bool TryGetGetterAndSetter(SerializedProperty property, out Func<object> getter, out Action<object> setter)
        {
            return GetGetterAndSetter(property, out getter, out setter);
        }

        private static VisualElement BindChange<T>(BaseField<T> field, Action<object, object> onChanged)
        {
            field.RegisterValueChangedCallback((change) => onChanged?.Invoke(change.previousValue, change.newValue));
            return field;
        }

        // having fun here
        private static bool GetGetterAndSetter(SerializedProperty property, out Func<object> getter, out Action<object> setter)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    getter = () => property.intValue;
                    setter = (value) => property.intValue = (int)value;
                    return true;
                case SerializedPropertyType.Boolean:
                    getter = () => property.boolValue;
                    setter = (value) => property.boolValue = (bool)value;
                    return true;
                case SerializedPropertyType.Float:
                    getter = () => property.floatValue;
                    setter = (value) => property.floatValue = (float)value;
                    return true;
                case SerializedPropertyType.String:
                    getter = () => property.stringValue;
                    setter = (value) => property.stringValue = (string)value;
                    return true;
                case SerializedPropertyType.Color:
                    getter = () => property.colorValue;
                    setter = (value) => property.colorValue = (Color)value;
                    return true;
                case SerializedPropertyType.ObjectReference:
                    getter = () => property.objectReferenceValue;
                    setter = (value) => property.objectReferenceValue = (UnityEngine.Object)value;
                    return true;
                case SerializedPropertyType.LayerMask:
                    getter = () => (LayerMask)property.intValue;
                    setter = (value) => property.intValue = (LayerMask)value;
                    return true;
                case SerializedPropertyType.Enum:
                    getter = () => property.enumValueIndex;
                    setter = (value) => property.enumValueIndex = (int)value;
                    return true;
                case SerializedPropertyType.Vector2:
                    getter = () => property.vector2Value;
                    setter = (value) => property.vector2Value = (Vector2)value;
                    return true;
                case SerializedPropertyType.Vector3:
                    getter = () => property.vector3Value;
                    setter = (value) => property.vector3Value = (Vector3)value;
                    return true;
                case SerializedPropertyType.Vector4:
                    getter = () => property.vector4Value;
                    setter = (value) => property.vector4Value = (Vector4)value;
                    return true;
                case SerializedPropertyType.Rect:
                    getter = () => property.rectValue;
                    setter = (value) => property.rectValue = (Rect)value;
                    return true;
                case SerializedPropertyType.ArraySize:
                    getter = () => property.arraySize;
                    setter = (value) => property.arraySize = (int)value;
                    return true;
                case SerializedPropertyType.AnimationCurve:
                    getter = () => property.animationCurveValue;
                    setter = (value) => property.animationCurveValue = (AnimationCurve)value;
                    return true;
                case SerializedPropertyType.Bounds:
                    getter = () => property.boundsValue;
                    setter = (value) => property.boundsValue = (Bounds)value;
                    return true;
                case SerializedPropertyType.Vector2Int:
                    getter = () => property.vector2IntValue;
                    setter = (value) => property.vector2IntValue = (Vector2Int)value;
                    return true;
                case SerializedPropertyType.Vector3Int:
                    getter = () => property.vector3IntValue;
                    setter = (value) => property.vector3IntValue = (Vector3Int)value;
                    return true;
                case SerializedPropertyType.RectInt:
                    getter = () => property.rectIntValue;
                    setter = (value) => property.rectIntValue = (RectInt)value;
                    return true;
                case SerializedPropertyType.BoundsInt:
                    getter = () => property.boundsIntValue;
                    setter = (value) => property.boundsIntValue = (BoundsInt)value;
                    return true;
                case SerializedPropertyType.ManagedReference:
                    getter = () => PropertyValueHelper.GetTargetObjectOfProperty(property);
                    setter = (value) => property.managedReferenceValue = value;
                    return true;
                case SerializedPropertyType.Generic:
                    getter = () => PropertyValueHelper.GetTargetObjectOfProperty(property);
                    setter = (value) => PropertyValueHelper.SetTargetObjectOfProperty(property, value);
                    return true;
                default:
                    getter = null;
                    setter = null;
                    return false;
            }
        }

        // Ah shit...
        private static VisualElement CreateFieldByPropertyType(SerializedProperty property, object curValue, Action<object, object> onValueChanged)
        {
            var type = property.propertyType;

            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return BindChange(new IntegerField() { value = (int)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Boolean:
                    return BindChange(new Toggle() { value = (bool)curValue }, onValueChanged);
                case SerializedPropertyType.Float:
                    return BindChange(new FloatField() { value = (float)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.String:
                    return BindChange(new TextField() { value = (string)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Color:
                    return BindChange(new ColorField() { value = (Color)curValue }, onValueChanged);
                case SerializedPropertyType.ObjectReference:
                    PropertyValueHelper.GetTargetObjectOfProperty(property, out var fieldType);
                    return BindChange(new ObjectField() { value = (UnityEngine.Object)curValue, objectType = fieldType }, onValueChanged);
                case SerializedPropertyType.LayerMask:
                    return BindChange(new LayerMaskField() { value = (LayerMask)curValue }, onValueChanged);
                case SerializedPropertyType.Enum:
                    return BindChange(new EnumField(defaultValue: (Enum)PropertyValueHelper.GetTargetObjectOfProperty(property)), onValueChanged);
                case SerializedPropertyType.Vector2:
                    return BindChange(new Vector2Field() { value = (Vector2)curValue }, onValueChanged);
                case SerializedPropertyType.Vector3:
                    return BindChange(new Vector3Field() { value = (Vector3)curValue }, onValueChanged);
                case SerializedPropertyType.Vector4:
                    return BindChange(new Vector4Field() { value = (Vector4)curValue }, onValueChanged);
                case SerializedPropertyType.Rect:
                    return BindChange(new RectField() { value = (Rect)curValue }, onValueChanged);
                case SerializedPropertyType.ArraySize:
                    return BindChange(new IntegerField() { value = (int)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Character:
                    return BindChange(new TextField(1, false, false, '.') { value = (string)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.AnimationCurve:
                    return BindChange(new CurveField() { value = (AnimationCurve)curValue }, onValueChanged);
                case SerializedPropertyType.Bounds:
                    return BindChange(new BoundsField() { value = (Bounds)curValue }, onValueChanged);
                case SerializedPropertyType.Vector2Int:
                    return BindChange(new Vector2IntField() { value = (Vector2Int)curValue }, onValueChanged);
                case SerializedPropertyType.Vector3Int:
                    return BindChange(new Vector3IntField() { value = (Vector3Int)curValue }, onValueChanged);
                case SerializedPropertyType.RectInt:
                    return BindChange(new RectIntField() { value = (RectInt)curValue }, onValueChanged);
                case SerializedPropertyType.BoundsInt:
                    return BindChange(new BoundsIntField() { value = (BoundsInt)curValue }, onValueChanged);
                default:
                    return NotSupportedLabel;
            }
        }
    }
}
