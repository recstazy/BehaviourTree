using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Value/Copy Value")]
    public class CopyValue : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private BlackboardName from;

        [SerializeField]
        private BlackboardName to;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Copy {from} to {to}";
        }

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue(from, out var value))
            {
                Blackboard.SetValue(to, value);
            }

            yield return null;
        }
    }
}
