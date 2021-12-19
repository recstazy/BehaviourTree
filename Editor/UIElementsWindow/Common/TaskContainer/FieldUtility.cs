using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Reflection;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public static class FieldUtility
    {
        public static readonly VisualElement NullLabel = new Label("None");
        public static readonly VisualElement ComplexLabel = new Label("Complex Field");
        public static readonly VisualElement QuaternionLabel = new Label("Quaternions not supported");
        
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

        public static bool IsSerialized(this FieldInfo info)
        {
            if (info.IsPublic) return true;
            else if (info.GetCustomAttribute<SerializeField>() != null) return true;
            else if (info.GetCustomAttribute<SerializeReference>() != null) return true;

            return false;
        }

        public static VisualElement GetFieldByType(SerializedProperty property, Action<object> onValueChanged)
        {
            var type = property.propertyType;
            if (IsComplex(property.propertyType)) return ComplexLabel;

            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return BindChange(new IntegerField(property.displayName), onValueChanged);
                case SerializedPropertyType.Boolean:
                    return BindChange(new Toggle(property.displayName), onValueChanged);
                case SerializedPropertyType.Float:
                    return BindChange(new FloatField(property.displayName), onValueChanged);
                case SerializedPropertyType.String:
                    return BindChange(new TextField(property.displayName), onValueChanged);
                case SerializedPropertyType.Color:
                    return BindChange(new ColorField(property.displayName), onValueChanged);
                case SerializedPropertyType.ObjectReference:
                    return BindChange(new ObjectField(property.displayName), onValueChanged);
                case SerializedPropertyType.LayerMask:
                    return BindChange(new LayerMaskField(property.displayName), onValueChanged);
                case SerializedPropertyType.Enum:
                    return BindChange(new EnumField(property.displayName), onValueChanged);
                case SerializedPropertyType.Vector2:
                    return BindChange(new Vector2Field(property.displayName), onValueChanged);
                case SerializedPropertyType.Vector3:
                    return BindChange(new Vector3Field(property.displayName), onValueChanged);
                case SerializedPropertyType.Vector4:
                    return BindChange(new Vector4Field(property.displayName), onValueChanged);
                case SerializedPropertyType.Rect:
                    return BindChange(new RectField(property.displayName), onValueChanged);
                case SerializedPropertyType.ArraySize:
                    return BindChange(new IntegerField(property.displayName), onValueChanged);
                case SerializedPropertyType.Character:
                    return BindChange(new TextField(property.displayName, 1, false, false, '.'), onValueChanged);
                case SerializedPropertyType.AnimationCurve:
                    return BindChange(new CurveField(property.displayName), onValueChanged);
                case SerializedPropertyType.Bounds:
                    return BindChange(new BoundsField(property.displayName), onValueChanged);
                case SerializedPropertyType.Gradient:
                    return BindChange(new GradientField(property.displayName), onValueChanged);
                case SerializedPropertyType.Quaternion:
                    return QuaternionLabel;
                case SerializedPropertyType.Vector2Int:
                    return BindChange(new Vector2IntField(property.displayName), onValueChanged);
                case SerializedPropertyType.Vector3Int:
                    return BindChange(new Vector3IntField(property.displayName), onValueChanged);
                case SerializedPropertyType.RectInt:
                    return BindChange(new RectIntField(property.displayName), onValueChanged);
                case SerializedPropertyType.BoundsInt:
                    return BindChange(new BoundsIntField(property.displayName), onValueChanged);
                default:
                    return null;
            }
        }

        private static VisualElement BindChange<T>(BaseField<T> field, Action<object> onChanged)
        {
            field.RegisterValueChangedCallback((change) => onChanged?.Invoke(change));
            return field;
        }
    }
}
