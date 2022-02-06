using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class TreeSelector : VisualElement, IPlaymodeDependent
    {
        public new class UxmlFactory : UxmlFactory<TreeSelector> { }
        public static event System.Action<BehaviourTree> OnTreeChanged;

        #region Fields

        private Label _currentName;
        private VisualElement _dropdownElement;

        private static TreePlayer[] s_treePlayers;
        private static string[] s_playersNames;
        private static int s_currentIndex;
        private static string s_lastSelectedName;
        private static bool s_isPlaymode;

        private const string NoTargetText = "No Target";

        #endregion

        #region Properties

        public static TreePlayer CurrentPlayer => s_treePlayers[s_currentIndex];

        #endregion

        public TreeSelector()
        {
            var targetLabel = new Label("Target:");
            targetLabel.AddToClassList("target-label");
            Add(targetLabel);

            _dropdownElement = new VisualElement();
            _dropdownElement.AddToClassList("tree-selector-dropdown");
            Add(_dropdownElement);

            _currentName = new Label(NoTargetText);
            _dropdownElement.Add(_currentName);

            var manipulator = new ContextualMenuManipulator(CreateMenu);
            this.AddManipulator(manipulator);
        }

        public void PlaymodeChanged(bool isPlaymode)
        {
            if (isPlaymode)
            {
                TargetChanged_Instance();
                OnTreeChanged += TargetChanged_Internal;
            }

            SetEnabled(isPlaymode);
            PlaymodeChanged_Internal(isPlaymode);

            if (!isPlaymode) OnTreeChanged -= TargetChanged_Internal;
        }

        private static void PlaymodeChanged_Internal(bool isPlaymode)
        {
            if (isPlaymode != s_isPlaymode)
            {
                s_isPlaymode = isPlaymode;

                if (isPlaymode)
                {
                    UpdatePlayers();
                    BehaviourPlayer.OnInstancedOrDestroyed += UpdatePlayers;
                }
                else
                {
                    OnTreeChanged?.Invoke(null);
                    BehaviourPlayer.OnInstancedOrDestroyed -= UpdatePlayers;
                    s_playersNames = null;
                    s_treePlayers = null;
                    s_currentIndex = 0;
                }
            }
        }

        private static void SetNewTarget(int index)
        {
            if (s_currentIndex == index) return;

            s_currentIndex = index;
            OnTreeChanged?.Invoke(CurrentPlayer?.Tree);
        }

        private void CreateMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopImmediatePropagation();

            for (int i = 0; i < s_playersNames.Length; i++)
            {
                evt.menu.AppendAction(s_playersNames[i], TargetSelected, StatusCallback, i);
            }
        }

        private void TargetSelected(DropdownMenuAction action)
        {
            s_lastSelectedName = s_playersNames[(int)action.userData];
            SetNewTarget((int)action.userData);
        }

        private void TargetChanged_Internal(BehaviourTree newTree)
        {
            TargetChanged_Instance();
        }

        private void TargetChanged_Instance()
        {
            string currentTargetText = NoTargetText;

            if (s_currentIndex > 0)
            {
                currentTargetText = s_playersNames[s_currentIndex];
            }

            _currentName.text = currentTargetText;
        }

        private DropdownMenuAction.Status StatusCallback(DropdownMenuAction action)
        {
            return DropdownMenuAction.Status.Normal;
        }

        private static void UpdatePlayers()
        {
            s_treePlayers = new TreePlayer[1].Concat(TreePlayer.PlayersCache).ToArray();
            s_playersNames = s_treePlayers.Select(a => a == null ? "Empty" : a.FullName).ToArray();
            s_currentIndex = System.Array.IndexOf(s_playersNames, s_lastSelectedName);
            if (s_currentIndex < 0) s_currentIndex = 0;
            OnTreeChanged?.Invoke(CurrentPlayer?.Tree);
        }
    }
}
