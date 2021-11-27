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
        private BlackboardGetter _from;

        [SerializeField]
        private BlackboardSetter _to;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Copy {_from} to {_to}";
        }

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue(_from, out object value))
            {
                Blackboard.TrySetValue(_to, value);
            }

            yield return null;
        }
    }
}
