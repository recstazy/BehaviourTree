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

        private static readonly Color s_entryColor = new Color(0.5f, 1f, 0.7f, 1f);
        private static readonly Vector2 s_entrySize = new Vector2(100f, 30f);
        private GUIStyle _style;

        #endregion

        #region Properties

        protected override Vector2 Size => s_entrySize;

        #endregion

        public EntryTreeNode(NodeData data) : base(data) { }

        protected override void DrawContent(Rect transformedRect)
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.alignment = TextAnchor.MiddleCenter;
                _style.fontSize = 18;
                _style.fontStyle = FontStyle.Bold;
                _style.normal.textColor = s_entryColor;
            }

            EditorGUI.LabelField(transformedRect, "Entry", _style);
        }
    }
}
