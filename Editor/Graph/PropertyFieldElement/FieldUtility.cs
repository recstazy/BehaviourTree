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

        public static bool IsComplex(SerializedProperty sProp)
        {
            var type = sProp.propertyType;

            switch (type)
            {
                case SerializedPropertyType.Generic:
                    return true;
                case SerializedPropertyType.ManagedReference:
                    {
                        var objValue = PropertyValueHelper.GetTargetObjectOfProperty(sProp);
                        var simpleDrawer = GetSimpleDrawerForManagedReference(objValue?.GetType(), objValue, null);
                        return simpleDrawer == null;
                    }
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
            if (IsComplex(property)) return ComplexLabel;
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

        public static bool CreateSerializedObjectAndProperty(UnityEngine.Object target, string path, out SerializedObject serializedObject, out SerializedProperty property)
        {
            if (target != null && !string.IsNullOrEmpty(path))
            {
                serializedObject = new SerializedObject(target);
                property = serializedObject.FindProperty(path);
                return true;
            }
            else
            {
                serializedObject = null;
                property = null;
                return false;
            }
        }

        private static VisualElement BindChange<T>(BaseField<T> field, Action<object, object> onChanged)
        {
            field.RegisterValueChangedCallback((change) => onChanged?.Invoke(change.previousValue, change.newValue));
            return field;
        }

        // having fun here
        public static object GetValue(UnityEngine.Object target, string serializedPropertyPath)
        {
            SerializedObject sObject;
            SerializedProperty property;
            CreateSerializedObjectAndProperty(target, serializedPropertyPath, out sObject, out property);
            if (sObject == null || property == null) return null;

            object result;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    result = property.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    result = property.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    result = property.floatValue;
                    break;
                case SerializedPropertyType.String:
                    result = property.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    result = property.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    result = property.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    result = (LayerMask)property.intValue;
                    break;
                case SerializedPropertyType.Enum:
                    result = property.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    result = property.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    result = property.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    result = property.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    result = property.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    result = property.arraySize;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    result = property.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    result = property.boundsValue;
                    break;
                case SerializedPropertyType.Vector2Int:
                    result = property.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    result = property.vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    result = property.rectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    result = property.boundsIntValue;
                    break;
                case SerializedPropertyType.ManagedReference:
                    result = PropertyValueHelper.GetTargetObjectOfProperty(property);
                    break;
                case SerializedPropertyType.Generic:
                    result = PropertyValueHelper.GetTargetObjectOfProperty(property);
                    break;
                default:
                    result = null;
                    break;
            }

            property.Dispose();
            sObject.Dispose();
            return result;
        }

        // nice
        public static void SetValue(UnityEngine.Object target, string serializedPropertyPath, object value)
        {
            SerializedObject sObject;
            SerializedProperty property;
            CreateSerializedObjectAndProperty(target, serializedPropertyPath, out sObject, out property);
            if (sObject == null || property == null) return;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)value;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = (LayerMask)value;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = (int)value;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4)value;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = (int)value;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = (AnimationCurve)value;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds)value;
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = (Vector2Int)value;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = (Vector3Int)value;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = (RectInt)value;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = (BoundsInt)value;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = value;
                    break;
                case SerializedPropertyType.Generic:
                    PropertyValueHelper.SetTargetObjectOfProperty(property, value);
                    break;
            }

            sObject.ApplyModifiedProperties();
            property.Dispose();
            sObject.Dispose();
        }

        // woa
        private static VisualElement CreateFieldByPropertyType(SerializedProperty property, object curValue, Action<object, object> onValueChanged)
        {
            var simpleField = CreateFieldByPropertyTypeSimple(property.propertyType, curValue, onValueChanged);
            if (simpleField != null) return simpleField;
            var type = property.propertyType;

            switch (type)
            {
                case SerializedPropertyType.Color:
                    return BindChange(new ColorField() { value = (Color)curValue }, onValueChanged);
                case SerializedPropertyType.ObjectReference:
                    PropertyValueHelper.GetTargetObjectOfProperty(property, out Type fieldType);
                    return BindChange(new ObjectField() { value = (UnityEngine.Object)curValue, objectType = fieldType, allowSceneObjects = false }, onValueChanged);
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
                case SerializedPropertyType.Enum:
                    return BindChange(new EnumField(defaultValue: (Enum)PropertyValueHelper.GetTargetObjectOfProperty(property)), onValueChanged);
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
                case SerializedPropertyType.ManagedReference:
                    var objValue = PropertyValueHelper.GetTargetObjectOfProperty(property);
                    var simpleDrawer = GetSimpleDrawerForManagedReference(objValue?.GetType(), objValue, onValueChanged);
                    if (simpleDrawer != null) return simpleDrawer;
                    else return NotSupportedLabel;
                default:
                    return NotSupportedLabel;
            }
        }

        private static VisualElement CreateFieldByPropertyTypeSimple(SerializedPropertyType type, object curValue, Action<object, object> onValueChanged)
        {
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
                case SerializedPropertyType.LayerMask:
                    return BindChange(new LayerMaskField() { value = (LayerMask)curValue }, onValueChanged);
                case SerializedPropertyType.Character:
                    return BindChange(new TextField(1, false, false, '.') { value = (string)curValue, isDelayed = true }, onValueChanged);
                default:
                    return null;
            }
        }

        private static VisualElement GetSimpleDrawerForManagedReference(Type objType, object curValue, Action<object, object> onValueChanged)
        {
            if (objType == null) return null;
            var typeCode = Type.GetTypeCode(objType);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return CreateFieldByPropertyTypeSimple(SerializedPropertyType.Boolean, curValue, onValueChanged);
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return CreateFieldByPropertyTypeSimple(SerializedPropertyType.Integer, curValue, onValueChanged);
                case TypeCode.Char:
                    return CreateFieldByPropertyTypeSimple(SerializedPropertyType.Character, curValue, onValueChanged);
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return CreateFieldByPropertyTypeSimple(SerializedPropertyType.Float, curValue, onValueChanged);
                case TypeCode.String:
                    return CreateFieldByPropertyTypeSimple(SerializedPropertyType.String, curValue, onValueChanged);
            }

            if (objType.IsEnum)
            {
                var enumIndex = (int)curValue;
                var value = Enum.GetValues(objType).GetValue(enumIndex);
                return BindChange(new EnumField(defaultValue: (Enum)value), onValueChanged);
            }

            return null;
        }
    }
}
