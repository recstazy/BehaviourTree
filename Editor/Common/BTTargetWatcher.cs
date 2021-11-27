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

        private const float LeftSpacing = 15f;
        private const float RectHeight = 20f;
        private const float LabelWidth = 30f;
        private const float DropDownWidth = 200f;
        private const float NoTreeWidth = 100f;

        private static bool s_isPlaying;
        private static BehaviourPlayer[] s_allPlayers;
        private static string[] s_playersNames;
        private static int s_currentIndex;

        private int _lastIndex;
        private Rect _rect;
        private GUIStyle _labelStyle;
        private GUIStyle _dropDownStyle;
        private BehaviourPlayer _current;

        #endregion

        #region Properties
		
        public static BehaviourPlayer CurrentPlayer { get; private set; }
        public BehaviourPlayer Current { get => _current; set { _current = value; CurrentPlayer = value; } }
        public Rect Rect => _rect;

        #endregion

        #region Static

        static BTTargetWatcher()
        {
            EditorApplication.playModeStateChanged += PlaymodeChanged;
            PlaymodeChanged(s_isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.ExitingPlayMode);
        }

        public static void UpdateIsPlaymode()
        {
            s_isPlaying = Application.isPlaying;
        }

        private static void UpdatePlayers()
        {
            s_allPlayers = new BehaviourPlayer[1].Concat(Object.FindObjectsOfType<BehaviourPlayer>()).ToArray();
            s_playersNames = s_allPlayers.Select(a => a == null ? "Empty" : a.gameObject.name).ToArray();

            if (CurrentPlayer != null)
            {
                int newIndex = System.Array.IndexOf(s_allPlayers, CurrentPlayer);
                s_currentIndex = Mathf.Clamp(newIndex, 0, s_allPlayers.Length);
            }
        }

        private static void PlaymodeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                s_isPlaying = true;
                UpdatePlayers();
                BehaviourPlayer.OnInstancedOrDestroyed += UpdatePlayers;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                s_isPlaying = false;
                BehaviourPlayer.OnInstancedOrDestroyed -= UpdatePlayers;
                ClearStaticData();
            }
        }

        private static void ClearStaticData()
        {
            s_playersNames = null;
            s_allPlayers = null;
            s_currentIndex = 0;
        }

        #endregion

        public BTTargetWatcher()
        {
            _rect.height = RectHeight;
            
            if (s_isPlaying)
            {
                UpdatePlayers();
                s_currentIndex = Mathf.Clamp(s_currentIndex, 0, s_allPlayers.Length - 1);
                Current = s_allPlayers[s_currentIndex];
            }
        }

        public void Dispose()
        {
            ClearRuntimeData();
        }

        public void OnGUI(Rect windowRect)
        {
            if (s_isPlaying)
            {
                _rect.width = windowRect.width;

                if (_labelStyle == null)
                {
                    CreateGUI();
                }

                Rect currentRect = _rect;
                currentRect.position += Vector2.right * LeftSpacing;
                currentRect.width = LabelWidth;
                EditorGUI.LabelField(currentRect, "Target", _labelStyle);

                currentRect.position += Vector2.right * (currentRect.width + 10f);
                currentRect.width = DropDownWidth;

                int newIndex = s_currentIndex;
                newIndex = EditorGUI.Popup(currentRect, newIndex, s_playersNames, _dropDownStyle);

                if (newIndex != _lastIndex)
                {
                    ChangeTarget(newIndex);
                }

                if (s_currentIndex != 0 && Current?.Tree == null)
                {
                    currentRect.x += currentRect.width;
                    currentRect.width = NoTreeWidth;
                    EditorGUI.HelpBox(currentRect, "No Tree", MessageType.Error);
                }
            }
        }

        private void ChangeTarget(int index)
        {
            if (index != 0)
            {
                s_currentIndex = index;

                if (s_currentIndex >= 0)
                {
                    Current = s_allPlayers[s_currentIndex];
                }

                _lastIndex = s_currentIndex;
            }

            GUI.changed = true;
        }

        private void CreateGUI()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.alignment = TextAnchor.MiddleCenter;
            _labelStyle.normal.textColor = Color.white;

            _dropDownStyle = (GUIStyle)"ToolbarCreateAddNewDropDown";
            _dropDownStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void ClearRuntimeData()
        {
            Current = null;
        }
    }
}
