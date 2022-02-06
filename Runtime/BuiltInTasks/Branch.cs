using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskMenu("Branch")]
    [TaskOut("True"), TaskOut("False")]
    public class Branch : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private InputValue<bool> _bool;

        [SerializeField]
        private bool _invert;

        #endregion

        #region Properties

        #endregion

        protected override int GetNextOutIndex()
        {
            bool predicate = _invert ? !_bool.Value : _bool.Value;
            return predicate ? 0 : 1;
        }

        protected override IEnumerator TaskRoutine()
        {
            Succeed = true;
            yield break;
        }
    }
}
