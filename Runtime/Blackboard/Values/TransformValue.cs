using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard transform representation
    /// </summary>
    [System.Serializable]
    public class TransformValue : ComponentValue<Transform>
    {
        public TransformValue() { }
        public TransformValue(Transform value) : base(value) { }
    }
}
