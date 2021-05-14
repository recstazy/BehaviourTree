using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard Rigidbody representation
    /// </summary>
    [System.Serializable]
    public class RigidbodyValue : ObjectValue<Rigidbody>
    {
        public RigidbodyValue() { }
        public RigidbodyValue(Rigidbody value) : base(value) { }
    }
}
