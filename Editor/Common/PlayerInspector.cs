using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomEditor(typeof(BehaviourPlayer), true)]
    [CanEditMultipleObjects]
    public class PlayerInspector : Editor
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawButtons();
        }

        private void DrawButtons()
        {
            if(GUILayout.Button("Fetch Components"))
            {
                var player = target as BehaviourPlayer;
                player.FetchCommonValues();
                EditorUtility.SetDirty(player);
                EditorUtility.SetDirty(player.SharedTree.Blackboard);
            }
        }
    }
}
