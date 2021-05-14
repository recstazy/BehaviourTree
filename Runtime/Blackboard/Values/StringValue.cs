using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Blackboard string representation
    /// </summary>
    [System.Serializable]
    public class StringValue : ObjectValue<string>
    {
        public StringValue() { }
        public StringValue(string value) : base(value) { }
    }
}
