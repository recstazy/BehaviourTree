using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class Pin
    {
        #region Fields

        protected Rect _rect;

        #endregion

        #region Properties

        public Rect Rect => _rect;
        public string Name { get; set; } = "";
        public int Index { get; protected set; }

        #endregion

        public Pin(int index)
        {
            Index = index;
        }

        public virtual void OnGUI(Vector2 posOnNodeBorder, float width)
        {
            _rect.center = posOnNodeBorder;
        }
    }
}
