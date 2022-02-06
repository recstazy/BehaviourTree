using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyDrawer(typeof(BlackboardName), true)]
    internal class BBNameDrawer : PropertyDrawer
    {
        #region Fields

        private Rect _rect;
        private SerializedProperty _property;
        private GUIContent _label;
        private Blackboard _currentBB;

        #endregion

        #region Properties

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _rect = position;
            _property = property;
            _label = label;

            if (!property.serializedObject.targetObject.TryGetBlackboard(out _currentBB))
            {
                EditorGUI.LabelField(_rect, "[No blackboard]");
            }
            else
            {
                DrawEnumedName();
            }
        }

        private void DrawEnumedName()
        {
            var rect = _rect;
            var valueTypings = fieldInfo.GetCustomAttributes(typeof(ValueTypeAttribute), true) as ValueTypeAttribute[];
            bool hasTyping = valueTypings != null && valueTypings.Length > 0;
            BlackboardProperty propType = _property.type.Contains("Setter") ? BlackboardProperty.Setter : BlackboardProperty.Getter;
            string[] names = hasTyping ? _currentBB.GetNamesTyped(propType, valueTypings[0].CompatableTypes) : _currentBB.GetNames(propType);

            if (names != null && names.Length > 0)
            {
                var options = names.Select(n => new GUIContent(n)).ToArray();
                var currentNameProp = _property.FindPropertyRelative("_name");
                int currentNameIndex = Array.IndexOf(names, currentNameProp.stringValue);
                currentNameIndex = Mathf.Clamp(currentNameIndex, 0, names.Length - 1);

                currentNameIndex = EditorGUI.Popup(rect, _label, currentNameIndex, options);
                currentNameProp.stringValue = names[currentNameIndex];
            }
            else
            {
                string message = hasTyping ? "No Compatable value" : "Blackboard Empty";
                EditorGUI.LabelField(rect, _label, $"[{message}]");
            }
        }
    }
}
