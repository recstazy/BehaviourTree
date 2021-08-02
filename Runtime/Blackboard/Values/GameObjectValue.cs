using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard GameObject representation
    /// </summary>
    [System.Serializable]
    public class GameObjectValue : ObjectValue<GameObject>, IGameObjectProvider
    {
        public GameObject gameObject => Value;

        public GameObjectValue() { }
        public GameObjectValue(GameObject value) : base(value) { }
    }
}
