using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTMousePosProvider : MouseManipulator
    {
        #region Fields

        #endregion

        #region Properties

        public Vector2 MousePosition { get; private set; }

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
            MousePosition = (target as BTGraph).viewTransform.matrix.inverse.MultiplyPoint3x4(evt.mousePosition);
        }
    }
}
