using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyDrawer(typeof(BlackboardName))]
    internal class BBNameDrawer : PropertyDrawer
    {
        #region Fields

        private Rect rect;
        private SerializedProperty property;
        private GUIContent label;
        private Blackboard currentBB;
        private const float nameValueRatio = 0.4f;

        #endregion

        #region Properties

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            rect = position;
            this.property = property;
            this.label = label;
            currentBB = GetBlackboard();

            if (currentBB is null)
            {
                EditorGUI.LabelField(rect, "[No blackboard]");
            }
            else
            {
                DrawEnumedName();
            }
        }

        private void DrawEnumedName()
        {
            var rect = this.rect;
            rect.width *= nameValueRatio;
            EditorGUI.LabelField(rect, label);

            rect.x += rect.width;
            rect.width = this.rect.width - rect.width;

            var valueTypings = fieldInfo.GetCustomAttributes(typeof(ValueTypeAttribute), true) as ValueTypeAttribute[];
            bool hasTyping = valueTypings != null && valueTypings.Length > 0;
            string[] names = hasTyping ? currentBB.GetNamesTyped(valueTypings[0].CompatableTypes) : currentBB.GetNames();

            if (names != null && names.Length > 0)
            {
                var currentNameProp = property.FindPropertyRelative("name");
                int currentNameIndex = Array.IndexOf(names, currentNameProp.stringValue);
                currentNameIndex = Mathf.Clamp(currentNameIndex, 0, names.Length - 1);

                currentNameIndex = EditorGUI.Popup(rect, currentNameIndex, names);
                currentNameProp.stringValue = names[currentNameIndex];
            }
            else
            {
                string message = hasTyping ? "No Compatable value" : "Blackboard Empty";
                EditorGUI.LabelField(rect, $"[{message}]");
            }
        }

        private Blackboard GetBlackboard()
        {
            var sObject = property.serializedObject;

            if (sObject.targetObject is IBlackboardProvider bbProvider)
            {
                return bbProvider.Blackboard;
            }
            else if (sObject.targetObject is Component component)
            {
                if (component.TryGetComponent<IBlackboardProvider>(out var provider))
                {
                    return provider.Blackboard;
                }
            }

            return null;
        }
    }
}
