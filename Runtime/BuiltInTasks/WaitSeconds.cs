using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Wait seconds and succeed
    /// </summary>
    [TaskOut(0)]
    public class WaitSeconds : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private float _time = 2f;

        #endregion

        #region Properties

        public float Time { get => _time; set => _time = value; }

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            yield return new WaitForSeconds(Time);
            Succeed = true;
        }
    }
}
