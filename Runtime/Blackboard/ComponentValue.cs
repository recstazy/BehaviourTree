using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public class ComponentValue<T> : ObjectValue<T>, IGameObjectProvider where T : Component
    {
        #region Fields

        #endregion

        #region Properties

        public GameObject gameObject => Value != null ? Value.gameObject : null;

        #endregion

        public ComponentValue() { }
        public ComponentValue(T value) : base(value) { }
    }
}
