using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class MousePositionProviderManipulator : MouseManipulator
    {
        #region Fields

        #endregion

        #region Properties

        public Vector2 MousePos { get; private set; }

        #endregion

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseMoveEvent>(MouseMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseMoveEvent>(MouseMove);
        }

        private void MouseMove(MouseMoveEvent evt)
        {
            MousePos = evt.mousePosition;
        }
    }
}
