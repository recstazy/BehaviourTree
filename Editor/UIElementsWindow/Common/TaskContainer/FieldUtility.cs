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

        // Ah shit...
        public static VisualElement GetFieldByType(SerializedPropertyType type, object curValue, Action<object, object> onValueChanged)
        {
            if (IsComplex(type)) return ComplexLabel;

            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return BindChange(new IntegerField() { value = (int)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Boolean:
                    return BindChange(new Toggle() { value = (bool) curValue }, onValueChanged);
                case SerializedPropertyType.Float:
                    return BindChange(new FloatField() { value = (float)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.String:
                    return BindChange(new TextField() { value = (string)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Color:
                    return BindChange(new ColorField() { value = (Color)curValue }, onValueChanged);
                case SerializedPropertyType.ObjectReference:
                    return BindChange(new ObjectField() { value = (UnityEngine.Object)curValue, objectType = curValue.GetType() }, onValueChanged);
                case SerializedPropertyType.LayerMask:
                    return BindChange(new LayerMaskField() { value = (LayerMask)curValue }, onValueChanged);
                case SerializedPropertyType.Enum:
                    return BindChange(new EnumField(defaultValue: (Enum)curValue),  onValueChanged);
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
                    return BindChange(new TextField(1, false, false, '.') {value = (string)curValue, isDelayed = true }, onValueChanged);
                case SerializedPropertyType.AnimationCurve:
                    return BindChange(new CurveField() { value = (AnimationCurve)curValue }, onValueChanged);
                case SerializedPropertyType.Bounds:
                    return BindChange(new BoundsField() { value = (Bounds)curValue }, onValueChanged);
                case SerializedPropertyType.Gradient:
                    return BindChange(new GradientField() { value = (Gradient)curValue }, onValueChanged);
                case SerializedPropertyType.Vector2Int:
                    return BindChange(new Vector2IntField() { value = (Vector2Int)curValue }, onValueChanged);
                case SerializedPropertyType.Vector3Int:
                    return BindChange(new Vector3IntField() { value = (Vector3Int)curValue }, onValueChanged);
                case SerializedPropertyType.RectInt:
                    return BindChange(new RectIntField() { value = (RectInt)curValue }, onValueChanged);
                case SerializedPropertyType.BoundsInt:
                    return BindChange(new BoundsIntField() { value = (BoundsInt)curValue}, onValueChanged);
                default:
                    return NotSupportedLabel;
            }
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

        private static VisualElement BindChange<T>(BaseField<T> field, Action<object, object> onChanged)
        {
            field.RegisterValueChangedCallback((change) => onChanged?.Invoke(change.previousValue, change.newValue));
            return field;
        }
    }
}
