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
    public class ListPropertyElement : BasePropertyFieldElement
    {
        #region Fields

        private ListViewWindow _window;
        private string _windowName;

        #endregion

        #region Properties

        public int ArraySize { get; private set; }

        #endregion

        public override void SetProperty(SerializedProperty property, bool hideLabelAndUnwrap = false)
        {
            base.SetProperty(property, true);
            _windowName = property.displayName;
            ArraySize = property.arraySize;
        }

        protected override void Detached(DetachFromPanelEvent evt)
        {
            base.Detached(evt);
            Close();
        }

        protected override void CreateVisualElements(SerializedProperty property)
        {
            var button = new Button(EditClicked);
            button.text = "Edit";
            FieldsContainer.Add(button);
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
            CallChanged();
        }
    }
}
