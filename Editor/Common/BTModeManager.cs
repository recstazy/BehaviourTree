using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTModeManager
    {
        #region Fields

        private static readonly Vector2 labelSize = new Vector2(40, 20);
        private const string playmodeLabel = "Player";
        private const string editorLabel = "Editor";
        
        private Rect labelRect;

        #endregion

        #region Properties
		
        public static bool IsPlaymode { get; private set; }

        #endregion

        public BTModeManager()
        {
            EditorApplication.playModeStateChanged += PlaymodeChanged;
            IsPlaymode = Application.isPlaying;
            labelRect = new Rect(Vector2.zero, labelSize);
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
            labelRect.position = new Vector2(windowRect.width - labelSize.x, 0f);
            EditorGUI.HelpBox(labelRect, IsPlaymode ? playmodeLabel : editorLabel, MessageType.None);
        }
    }
}
