using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut]
    [TaskMenu("Debug/Log Variable")]
    public class LogVariable : BaseLogTask
    {
        #region Fields

        [SerializeField]
        private BlackboardGetter _name;

        #endregion

        #region Properties

        #endregion

        protected override string GetLogString()
        {
            if (Blackboard.TryGetValue(_name, out var value))
            {
                if (value is Object uObject) return uObject != null ? uObject.name : "null";
                else return value != null ? value.ToString() : "null";
            }

            return $"No value with name '{_name}'";
        }
    }
}
