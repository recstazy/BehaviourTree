using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomEditor(typeof(BehaviourTree), false)]
    [CanEditMultipleObjects]
    public class BehaviourTreeDrawer : Editor
    {
        #region Fields

        private SerializedProperty _bbProperty;

        #endregion

        #region Properties

        #endregion

        public override void OnInspectorGUI()
        {
            _bbProperty = serializedObject.FindProperty("_blackboard");
            DrawBBProperty();
        }

        private void DrawBBProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_bbProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(20);
            DrawDefaultInspector();
        }
    }
}
