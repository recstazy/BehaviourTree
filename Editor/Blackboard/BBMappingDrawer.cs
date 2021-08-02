using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyDrawer(typeof(BlackboardValueMapping))]
    public class BBMappingDrawer : PropertyDrawer
    {
        #region Fields

        private Rect _rect;
        private SerializedProperty _property;
        private Blackboard _currentBB;

        #endregion

        #region Properties

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;
            _rect = position;

            if (!property.serializedObject.targetObject.TryGetBlackboard(out _currentBB))
            {
                EditorGUI.LabelField(_rect, "[No blackboard]");
            }
            else
            {
                DrawMapping();
            }
        }

        private void DrawMapping()
        {
            var nameProperty = _property.FindPropertyRelative("_name");
            Rect rect = _rect;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, nameProperty);

            var valueProp = _property.FindPropertyRelative("_value");
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height = EditorGUIUtility.singleLineHeight * 2;

            ITypedValue bbValue;
            _currentBB.TryGetValue(nameProperty.FindPropertyRelative("_name").stringValue, out bbValue);
            var drawer = new TypedValueDrawer();
            drawer.ForceSetType(bbValue == null ? null : bbValue.GetType(), true);
            drawer.OnGUI(rect, valueProp, new GUIContent("Value"));
        }
    }
}
