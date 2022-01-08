using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class TargetTreeWatcher : VisualElement, IPlaymodeDependent
    {
        public new class UxmlFactory : UxmlFactory<TargetTreeWatcher> { }

        #region Fields

        private Label _currentName;
        private VisualElement _dropdownElement;

        private static TreePlayer[] s_treePlayers;
        private static string[] s_playersNames;
        private static int s_currentIndex;
        private static string s_currentName;

        #endregion

        #region Properties

        public TreePlayer CurrentPlayer => s_treePlayers[s_currentIndex];

        #endregion

        public TargetTreeWatcher()
        {
            var targetLabel = new Label("Target:");
            targetLabel.AddToClassList("target-label");
            Add(targetLabel);

            _dropdownElement = new VisualElement();
            _dropdownElement.AddToClassList("target-watcher-dropdown");
            Add(_dropdownElement);

            _currentName = new Label("No Target");
            _dropdownElement.Add(_currentName);

            var manipulator = new ContextualMenuManipulator(CreateMenu);
            this.AddManipulator(manipulator);
        }

        public void PlaymodeChanged(bool isPlaymode)
        {
            if (isPlaymode)
            {
                UpdatePlayers();
                BehaviourPlayer.OnInstancedOrDestroyed += UpdatePlayers;
            }
            else
            {
                BehaviourPlayer.OnInstancedOrDestroyed -= UpdatePlayers;
                s_playersNames = null;
                s_treePlayers = null;
                s_currentIndex = 0;
                _currentName.text = "No Target";
            }

            SetEnabled(isPlaymode);
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
            SetNewTarget((int)action.userData);
        }

        private void SetNewTarget(int index)
        {
            s_currentIndex = index;
            s_currentName = s_playersNames[s_currentIndex];
            _currentName.text = s_currentName;
        }

        private DropdownMenuAction.Status StatusCallback(DropdownMenuAction action)
        {
            return DropdownMenuAction.Status.Normal;
        }

        private static void UpdatePlayers()
        {
            s_treePlayers = new TreePlayer[1].Concat(TreePlayer.PlayersCache).ToArray();
            s_playersNames = s_treePlayers.Select(a => a == null ? "Empty" : a.FullName).ToArray();
            s_currentIndex = System.Array.IndexOf(s_playersNames, s_currentName);
            if (s_currentIndex < 0) s_currentIndex = 0;
        }
    }
}
