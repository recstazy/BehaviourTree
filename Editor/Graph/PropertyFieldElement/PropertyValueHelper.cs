// Huge thanks to @lordofduct github

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public static class PropertyValueHelper
    {
        /// <summary> Gets the object the property represents. </summary>
        public static object GetTargetObjectOfProperty(SerializedProperty property)
        {
            return GetTargetObjectOfProperty(property, out FieldInfo fieldinfo);
        }

        public static object GetTargetObjectOfProperty(SerializedProperty property, out Type fieldType)
        {
            var value = GetTargetObjectOfProperty(property, out FieldInfo fieldInfo);
            fieldType = GetFieldType(fieldInfo);
            return value;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty property, out FieldInfo fieldInfo)
        {
            fieldInfo = null;
            if (property == null) return null;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            Type objectType;
            var elements = path.Split('.');
            string lastName = elements.Last();
            elements = elements.Take(elements.Length - 1).ToArray();

            foreach (var element in elements)
            {
                obj = GetValueByPropertyPathName(obj, element, out objectType);
            }

            var lastValue = GetValueByPropertyPathName(obj, lastName, out var lastObjectType);
            fieldInfo = GetFieldInfo_Imp(obj.GetType(), lastName);
            return lastValue;
        }

        public static Type GetFieldType(FieldInfo fieldInfo)
        {
            var fieldType = fieldInfo?.FieldType;

            if (fieldType != null && typeof(IEnumerable).IsAssignableFrom(fieldType))
            {
                fieldType = GetArrayElementType(fieldType);
            }

            return fieldType;
        }

        public static void SetTargetObjectOfProperty(SerializedProperty prop, object value)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            if (ReferenceEquals(obj, null)) return;

            try
            {
                var element = elements.Last();

                if (element.Contains("["))
                {
                    var tp = obj.GetType();
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    var field = tp.GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var arr = field.GetValue(obj) as System.Collections.IList;
                    if (arr != null) arr[index] = value;
                }
                else
                {
                    var tp = obj.GetType();
                    var field = tp.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(obj, value);
                }
            }
            catch { return; }
        }

        private static object GetValueByPropertyPathName(object source, string name, out Type objectType)
        {
            object value;

            if (name.Contains("["))
            {
                var elementName = name.Substring(0, name.IndexOf("["));
                var index = Convert.ToInt32(name.Substring(name.IndexOf("[")).Replace("[", "").Replace("]", ""));
                value = GetValue_Imp(source, elementName, index, out objectType);
            }
            else
            {
                value = GetValue_Imp(source, name, out objectType);
            }

            return value;
        }

        private static object GetValue_Imp(object source, string name)
        {
            return GetValue_Imp(source, name, out var type);
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            return GetValue_Imp(source, name, index, out var type);
        }

        private static object GetValue_Imp(object source, string name, out Type objectType)
        {
            objectType = null;
            if (source == null) return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (f != null)
                {
                    var value = f.GetValue(source);
                    objectType = value?.GetType();
                    return value;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index, out Type type)
        {
            type = null;
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }

            var current = enm.Current;
            type = current?.GetType();
            return current;
        }

        private static Type GetArrayElementType(Type enumerableType)
        {
            var interfaceType = enumerableType.GetInterface(typeof(IEnumerable<>).Name);
            return interfaceType.GenericTypeArguments.First();
        }

        private static FieldInfo GetFieldInfo_Imp(Type source, string name)
        {
            if (source == null) return null;
            var type = source;

            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
