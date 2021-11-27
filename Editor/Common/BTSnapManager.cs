using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTSnapManager
    {
        #region Fields

        private static readonly Vector2 s_rectSize = new Vector2(60, 20);
        private static readonly Vector2 s_padding = new Vector2(5, 0);
        private static readonly GUIContent s_toggleLabel = new GUIContent("Snap");

        #endregion

        #region Properties

        public static bool SnapEnabled { get; private set; } = true;
        public static int GridSize => 10;

        #endregion

        public BTSnapManager(bool enableSnap)
        {
            SnapEnabled = enableSnap;
        }

        public static Vector2 RoundToSnap(Vector2 vector)
        {
            return new Vector2(RoundToSnap(vector.x), RoundToSnap(vector.y));
        }

        public static float RoundToSnap(float value)
        {
            return Mathf.Round(value / GridSize) * GridSize;
        }

        public void OnGUI(Rect windowRect)
        {
            Rect rect = new Rect(new Vector2(0f, 0f), s_rectSize);
            EditorGUI.HelpBox(rect, "", MessageType.None);

            rect.position += s_padding;
            rect.size -= Vector2.right * s_padding;
            SnapEnabled = EditorGUI.ToggleLeft(rect, s_toggleLabel, SnapEnabled);
        }
    }
}
