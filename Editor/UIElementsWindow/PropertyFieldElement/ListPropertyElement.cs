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

        private UnityEngine.Object _serializedTargetObject;
        private string _propertyPath;
        private ListViewWindow _window;
        private string _windowName;

        #endregion

        #region Properties

        public int ArraySize { get; private set; }

        #endregion

        public void SetProperty(SerializedProperty property)
        {
            _serializedTargetObject = property.serializedObject.targetObject;
            _propertyPath = property.propertyPath;
            _windowName = property.displayName;
            ArraySize = property.arraySize;
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
            var window = ListViewWindow.Show(_serializedTargetObject, _propertyPath, _windowName);
            window.OnChanged += AnyFieldChanged;
            window.OnClosed += EditClosed;
            _window = window;
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
            ArraySize = _window.ArraySize;
            OnChanged?.Invoke();
        }
    }
}
