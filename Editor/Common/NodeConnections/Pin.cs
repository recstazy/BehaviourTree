using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class Pin
    {
        #region Fields

        private string name = "";
        protected Rect rect;

        #endregion

        #region Properties

        public Rect Rect => rect;
        public string Name { get => name; set => name = value; }
        public int Index { get; protected set; }

        #endregion

        public Pin(int index)
        {
            Index = index;
        }

        public virtual void OnGUI(Vector2 posOnNodeBorder, float width)
        {
            rect.center = posOnNodeBorder;
        }
    }
}
