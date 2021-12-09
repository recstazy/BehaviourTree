using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class Draggable : VisualElement
    {
        #region Fields

        private bool _isDown;
        private bool _isDragging;

        #endregion

        #region Properties

        #endregion

        public Draggable()
        {
            RegisterCallbacks();
        }

        protected virtual void MouseDown(MouseDownEvent evt) { }
        protected virtual void MouseUp(MouseUpEvent evt) { }
        protected virtual void EndDrag(MouseUpEvent evt) { }

        protected virtual void StartDrag(MouseMoveEvent evt) 
        {
            transform.position += (Vector3)evt.mouseDelta;
        }

        protected virtual void Drag(MouseMoveEvent evt)
        {
            transform.position += (Vector3)evt.mouseDelta;
        }

        private void RegisterCallbacks()
        {
            RegisterCallback<MouseDownEvent>(HandleMouseDown);
            RegisterCallback<MouseUpEvent>(HandleMouseUp);
            RegisterCallback<MouseMoveEvent>(HandleMouseMove);
        }

        private void HandleMouseDown(MouseDownEvent evt)
        {
            evt.StopImmediatePropagation();
            _isDown = true;
            MouseDown(evt);
        }

        private void HandleMouseUp(MouseUpEvent evt)
        {
            evt.StopImmediatePropagation();

            if (_isDragging) EndDrag(evt);
            _isDragging = false;
            _isDown = false;
            MouseUp(evt);
        }

        private void HandleMouseMove(MouseMoveEvent evt)
        {
            if (_isDown)
            {
                evt.StopImmediatePropagation();

                if (!_isDragging)
                {
                    _isDragging = true;
                    StartDrag(evt);
                }
                else Drag(evt);
            }
        }
    }
}
