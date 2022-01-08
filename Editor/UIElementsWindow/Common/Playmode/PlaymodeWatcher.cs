using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class PlaymodeWatcher : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PlaymodeWatcher> { }

        #region Fields

        private IPlaymodeDependent[] _dependencies;
        private Label _label;

        private const string PlaymodeLabel = "Player";
        private const string EditModeLabel = "Editor";

        #endregion

        #region Properties

        public static bool IsPlaymode { get; private set; }
        private string CurrentLabelText => IsPlaymode ? PlaymodeLabel : EditModeLabel;

        #endregion

        public PlaymodeWatcher()
        {
            RegisterCallback<DetachFromPanelEvent>(Detached);
            IsPlaymode = Application.isPlaying;
            EditorApplication.playModeStateChanged += PlaymodeCahanged;
            _label = new Label(CurrentLabelText);
            _label.AddToClassList("playmode-label");
            Add(_label);
        }

        public void SetDependencies(params IPlaymodeDependent[] dependencies)
        {
            _dependencies = dependencies;
            UpdatePlaymodeDependencies();
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            EditorApplication.playModeStateChanged -= PlaymodeCahanged;
            UnregisterCallback<DetachFromPanelEvent>(Detached);
            _dependencies = null;
        }

        private void PlaymodeCahanged(PlayModeStateChange state)
        {
            bool lastMode = IsPlaymode;

            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    IsPlaymode = false;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    IsPlaymode = true;
                    break;
            }

            if (lastMode != IsPlaymode) UpdatePlaymodeDependencies();
        }

        private void UpdatePlaymodeDependencies()
        {
            _label.text = CurrentLabelText;
            if (_dependencies == null) return;

            foreach (var dep in _dependencies)
            {
                dep.PlaymodeChanged(IsPlaymode);
            }
        }
    }
}
