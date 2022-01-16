using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Use this to show blackboard property "set" accessor in editor
    /// </summary>
    [System.Serializable]
    public class BlackboardSetter : BlackboardName 
    {
        public void Set(Blackboard blackboard, object value)
        {
            blackboard.TrySetValue(Name, value);
        }

        public void Set<T>(Blackboard blackboard, T value)
        {
            blackboard.TrySetValue(Name, value);
        }
    }
}
