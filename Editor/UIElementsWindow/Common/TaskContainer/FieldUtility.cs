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

        public static bool IsSerializedAndVisible(this FieldInfo info)
        {
            if (info.GetCustomAttribute<HideInInspector>() != null) return false;

            if (info.IsPublic) return true;
            else if (info.GetCustomAttribute<SerializeField>() != null) return true;
            else if (info.GetCustomAttribute<SerializeReference>() != null) return true;
            else return false;
        }

        public static VisualElement GetFieldByType(SerializedProperty property, Action<object, object> onValueChanged)
        {
            var type = property.propertyType;
            if (IsComplex(property.propertyType)) return ComplexLabel;

            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return BindChange(new IntegerField() { isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Boolean:
                    return BindChange(new Toggle(), onValueChanged);
                case SerializedPropertyType.Float:
                    return BindChange(new FloatField() { isDelayed = true }, onValueChanged);
                case SerializedPropertyType.String:
                    return BindChange(new TextField() { isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Color:
                    return BindChange(new ColorField(), onValueChanged);
                case SerializedPropertyType.ObjectReference:
                    return BindChange(new ObjectField(), onValueChanged);
                case SerializedPropertyType.LayerMask:
                    return BindChange(new LayerMaskField(), onValueChanged);
                case SerializedPropertyType.Enum:
                    return BindChange(new EnumField(), onValueChanged);
                case SerializedPropertyType.Vector2:
                    return BindChange(new Vector2Field(), onValueChanged);
                case SerializedPropertyType.Vector3:
                    return BindChange(new Vector3Field(), onValueChanged);
                case SerializedPropertyType.Vector4:
                    return BindChange(new Vector4Field(), onValueChanged);
                case SerializedPropertyType.Rect:
                    return BindChange(new RectField(), onValueChanged);
                case SerializedPropertyType.ArraySize:
                    return BindChange(new IntegerField() { isDelayed = true }, onValueChanged);
                case SerializedPropertyType.Character:
                    return BindChange(new TextField(1, false, false, '.') { isDelayed = true }, onValueChanged);
                case SerializedPropertyType.AnimationCurve:
                    return BindChange(new CurveField(), onValueChanged);
                case SerializedPropertyType.Bounds:
                    return BindChange(new BoundsField(), onValueChanged);
                case SerializedPropertyType.Gradient:
                    return BindChange(new GradientField(), onValueChanged);
                case SerializedPropertyType.Quaternion:
                    return QuaternionLabel;
                case SerializedPropertyType.Vector2Int:
                    return BindChange(new Vector2IntField(), onValueChanged);
                case SerializedPropertyType.Vector3Int:
                    return BindChange(new Vector3IntField(), onValueChanged);
                case SerializedPropertyType.RectInt:
                    return BindChange(new RectIntField(), onValueChanged);
                case SerializedPropertyType.BoundsInt:
                    return BindChange(new BoundsIntField(), onValueChanged);
                default:
                    return null;
            }
        }

        public static FieldInfo[] GetSerializedFieldsUpToBase(this Type type)
        {
            var baseType = type;
            List<FieldInfo> fields = new List<FieldInfo>();

            while (baseType != null)
            {
                var subProps = baseType
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                subProps = subProps.Where(p => p.IsSerializedAndVisible()).ToArray();

                foreach (var s in subProps)
                {
                    fields.Add(s);
                }

                baseType = baseType.BaseType;
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
