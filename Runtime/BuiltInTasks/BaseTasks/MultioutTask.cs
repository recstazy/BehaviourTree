using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base task to derive if you want a variable count of outputs
    /// </summary>
    [ExcludeFromTaskSelector]
    public class MultioutTask : BehaviourTask
    {
        #region Fields

        [SerializeField]
        [HideInInspector]
        private int outsCount;

        #endregion

        #region Properties

        public int OutsCount => outsCount;

        #endregion

        internal void ChangeOutsCount(int newCount)
        {
            outsCount = newCount;
        }
    }
}
