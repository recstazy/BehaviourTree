using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Compare two blackboard values and choose corresponding out
    /// </summary>
    [TaskOut(0, "Err"), TaskOut(1, "!="), TaskOut(2, "<"), TaskOut(3, "=="), TaskOut(4, ">")]
    [TaskMenu("Value/Compare Value")]
    public class CompareValue : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private BlackboardName ValueName;

        [SerializeReference]
        private ITypedValue compareWith;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Compare {ValueName}";
        }

        protected override int GetCurrentOutIndex()
        {
            if (Blackboard.TryGetValue(ValueName, out var bbValue))
            {
                var result = bbValue.Compare(compareWith);
                return (int)result;
            }

            return (int)CompareResult.TypeError;
        }
    }
}
