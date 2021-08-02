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
        private BlackboardName _from;

        [SerializeField]
        private BlackboardName _to;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Copy {_from} to {_to}";
        }

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue(_from, out var value))
            {
                Blackboard.SetValue(_to, value);
            }

            yield return null;
        }
    }
}
