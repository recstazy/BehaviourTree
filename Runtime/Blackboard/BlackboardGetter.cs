using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Use this to show blackboard property "get" accessor in editor
    /// </summary>
    [System.Serializable]
    public class BlackboardGetter : BlackboardName 
    {
        public bool TryGetValue(Blackboard blackboard, out object value)
        {
            return blackboard.TryGetValue(Name, out value);
        }

        public bool TryGetValue<T>(Blackboard blackboard, out T value)
        {
            return blackboard.TryGetValue<T>(Name, out value);
        }

        public object GetWithDefault(Blackboard blackboard)
        {
            if (blackboard.TryGetValue(Name, out var value)) return value;
            else return default;
        }

        public T GetWithDefault<T>(Blackboard blackboard, T defaultValue = default)
        {
            if (blackboard.TryGetValue<T>(Name, out var value)) return value;
            else return defaultValue;
        }
    }
}
