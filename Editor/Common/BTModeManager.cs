using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTModeManager
    {
        #region Fields

        private static readonly Vector2 s_labelSize = new Vector2(40, 20);
        private const string PlaymodeLabel = "Player";
        private const string EditorLabel = "Editor";
        private Rect _labelRect;

        #endregion

        #region Properties
		
        public static bool IsPlaymode { get; private set; }

        #endregion

        public BTModeManager()
        {
            EditorApplication.playModeStateChanged += PlaymodeChanged;
            IsPlaymode = Application.isPlaying;
            _labelRect = new Rect(Vector2.zero, s_labelSize);
        }

        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= PlaymodeChanged;
            IsPlaymode = false;
        }

        public void PlaymodeChanged(PlayModeStateChange state)
        {
            IsPlaymode = state == PlayModeStateChange.EnteredPlayMode;
        }

        public void OnGUI(Rect windowRect)
        {
            _labelRect.position = new Vector2(windowRect.width - s_labelSize.x, windowRect.height - s_labelSize.y);
            EditorGUI.HelpBox(_labelRect, IsPlaymode ? PlaymodeLabel : EditorLabel, MessageType.None);
        }
    }
}
