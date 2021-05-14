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
        private float time = 2f;

        #endregion

        #region Properties

        public float Time { get => time; set => time = value; }

        #endregion

        public override string GetDescription()
        {
            return $"Wait {time} seconds";
        }

        protected override IEnumerator TaskRoutine()
        {
            yield return new WaitForSeconds(Time);
            Succeed = true;
        }
    }
}
