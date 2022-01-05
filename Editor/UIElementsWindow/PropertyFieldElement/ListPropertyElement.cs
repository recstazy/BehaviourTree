using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Reflection;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class ListPropertyElement : VisualElement
    {
        public event Action OnChanged;

        #region Fields

        private SerializedProperty _listProperty;
        private ListViewWindow _window;

        #endregion

        #region Properties

        #endregion

        public void SetProperty(SerializedProperty property)
        {
            _listProperty = property;
            var button = new Button(EditClicked);
            button.text = "Edit";
            Add(button);
        }

        public void Close()
        {
            if (_window != null) _window.Close();
        }

        private void EditClicked()
        {
            var window = ListViewWindow.Show(_listProperty);
            window.OnChanged += AnyFieldChanged;
            window.OnClosed += EditClosed;
        }

        private void Unsubscribe()
        {
            if (_window != null)
            {
                _window.OnChanged -= AnyFieldChanged;
                _window.OnClosed -= EditClosed;
            }
        }

        private void EditClosed()
        {
            Unsubscribe();
            _window = null;
        }

        private void AnyFieldChanged()
        {
            OnChanged?.Invoke();
        }
    }
}
