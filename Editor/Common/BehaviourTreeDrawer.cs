using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomEditor(typeof(BehaviourTree), false)]
    [CanEditMultipleObjects]
    public class BehaviourTreeDrawer : Editor
    {
        #region Fields

        private SerializedProperty bbProperty;

        #endregion

        #region Properties

        #endregion

        public override void OnInspectorGUI()
        {
            bbProperty = serializedObject.FindProperty("blackboard");
            DrawBBProperty();
        }

        private void DrawBBProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(bbProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
