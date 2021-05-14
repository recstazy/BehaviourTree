using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomNodeDrawer(typeof(EntryTask))]
    internal class EntryTreeNode : BehaviourTreeNode
    {
        #region Fields

        private static readonly Color entryColor = new Color(0.5f, 1f, 0.7f, 1f);
        private static readonly Vector2 entrySize = new Vector2(100f, 30f);
        private GUIStyle style;

        #endregion

        #region Properties

        protected override Vector2 Size => entrySize;

        #endregion

        public EntryTreeNode(NodeData data) : base(data) { }

        protected override void DrawContent(Rect transformedRect)
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 18;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = entryColor;
            }

            EditorGUI.LabelField(transformedRect, "Entry", style);
        }
    }
}
