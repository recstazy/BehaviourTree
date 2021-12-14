using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyDrawer(typeof(BehaviourTask))]
    public class TaskPropertyDrawer : PropertyDrawer
    {
        #region Fields

        private const float NameToPropertyRatio = 0.5f;

        #endregion

        #region Properties

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0f;
            bool hasChildren = property.hasVisibleChildren;
            if (!hasChildren) return EditorGUIUtility.singleLineHeight;

            ForeachChildProperty(property, p => height += EditorGUI.GetPropertyHeight(p, true));
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool hasChildren = property.hasVisibleChildren;

            if (!hasChildren)
            {
                var taskName = property.managedReferenceFullTypename.Split('.').LastOrDefault();
                taskName = string.IsNullOrEmpty(taskName) ? "None" : taskName;
                EditorGUI.LabelField(position, $"{label.text} ({taskName})");
                return;
            }

            var rect = position;
            ForeachChildProperty(property, p => DrawPropertyField(rect, p, out rect));
        }

        private void DrawPropertyField(Rect currentRect, SerializedProperty property, out Rect rectChanged)
        {
            currentRect.height = EditorGUI.GetPropertyHeight(property);
            var rect = currentRect;
            rect.width *= NameToPropertyRatio;
            EditorGUI.LabelField(rect, property.displayName);
            rect.position += Vector2.right * rect.width;
            rect.width = currentRect.width - rect.width;
            EditorGUI.PropertyField(rect, property, GUIContent.none, true);
            currentRect.y += rect.height;
            rectChanged = currentRect;
        }

        private void ForeachChildProperty(SerializedProperty property, System.Action<SerializedProperty> action)
        {
            if (!property.hasVisibleChildren) return;
            property = property.Copy();
            
            if (property.NextVisible(true))
            {
                int depth = property.depth;

                do action?.Invoke(property);
                while (property.NextVisible(false) && depth == property.depth);
            }
        }
    }
}
