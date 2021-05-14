using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTTargetWatcher
    {
        #region Fields

        private const float leftSpacing = 15f;
        private const float rectHeight = 20f;
        private const float labelWidth = 30f;
        private const float dropDownWidth = 200f;
        private const float noTreeWidth = 100f;
        private static readonly Color backColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        private static bool isPlaying;
        private static BehaviourPlayer[] allPlayers;
        private static string[] playersNames;
        private static int currentIndex;
        private int lastIndex;

        private Rect rect;
        private GUIStyle labelStyle;
        private GUIStyle dropDownStyle;
        private BehaviourPlayer current;

        #endregion

        #region Properties
		
        public static BehaviourPlayer CurrentPlayer { get; private set; }
        public BehaviourPlayer Current { get => current; set { current = value; CurrentPlayer = value; } }
        public Rect Rect => rect;

        #endregion

        #region Static

        static BTTargetWatcher()
        {
            EditorApplication.playModeStateChanged += PlaymodeChanged;
            PlaymodeChanged(isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.ExitingPlayMode);
        }

        public static void UpdateIsPlaymode()
        {
            isPlaying = Application.isPlaying;
        }

        private static void UpdatePlayers()
        {
            allPlayers = new BehaviourPlayer[1].Concat(Object.FindObjectsOfType<BehaviourPlayer>()).ToArray();
            playersNames = allPlayers.Select(a => a is null ? "Empty" : a.gameObject.name).ToArray();

            if (CurrentPlayer != null)
            {
                int newIndex = System.Array.IndexOf(allPlayers, CurrentPlayer);
                currentIndex = Mathf.Clamp(newIndex, 0, allPlayers.Length);
            }
        }

        private static void PlaymodeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                isPlaying = true;
                UpdatePlayers();
                BehaviourPlayer.OnInstancedOrDestroyed += UpdatePlayers;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                isPlaying = false;
                BehaviourPlayer.OnInstancedOrDestroyed -= UpdatePlayers;
                ClearStaticData();
            }
        }

        private static void ClearStaticData()
        {
            playersNames = null;
            allPlayers = null;
            currentIndex = 0;
        }

        #endregion

        public BTTargetWatcher()
        {
            rect.height = rectHeight;
            
            if (isPlaying)
            {
                UpdatePlayers();
                currentIndex = Mathf.Clamp(currentIndex, 0, allPlayers.Length - 1);
                Current = allPlayers[currentIndex];
            }
        }

        public void Dispose()
        {
            ClearRuntimeData();
        }

        public void OnGUI(Rect windowRect)
        {
            if (isPlaying)
            {
                rect.width = windowRect.width;

                if (labelStyle == null)
                {
                    CreateGUI();
                }

                EditorGUI.DrawRect(rect, backColor);
                Rect currentRect = rect;
                currentRect.position += Vector2.right * leftSpacing;
                currentRect.width = labelWidth;
                EditorGUI.LabelField(currentRect, "Target", labelStyle);

                currentRect.position += Vector2.right * (currentRect.width + 10f);
                currentRect.width = dropDownWidth;

                int newIndex = currentIndex;
                newIndex = EditorGUI.Popup(currentRect, newIndex, playersNames, dropDownStyle);

                if (newIndex != lastIndex)
                {
                    ChangeTarget(newIndex);
                }

                if (currentIndex != 0 && Current?.Tree is null)
                {
                    currentRect.x += currentRect.width;
                    currentRect.width = noTreeWidth;
                    EditorGUI.HelpBox(currentRect, "No Tree", MessageType.Error);
                }
            }
        }

        private void ChangeTarget(int index)
        {
            if (index != 0)
            {
                currentIndex = index;

                if (currentIndex >= 0)
                {
                    Current = allPlayers[currentIndex];
                }

                lastIndex = currentIndex;
            }

            GUI.changed = true;
        }

        private void CreateGUI()
        {
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;

            dropDownStyle = (GUIStyle)"ToolbarCreateAddNewDropDown";
            dropDownStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void ClearRuntimeData()
        {
            Current = null;
        }
    }
}
