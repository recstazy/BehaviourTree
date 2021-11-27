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
        private int _outsCount;

        private static readonly Color s_backColor = new Color(0.1f, 0.1f, 0.175f, 0.5f);

        #endregion

        #region Properties

        public int OutsCount => _outsCount;
        protected override Color Color => s_backColor;

        #endregion

        internal void ChangeOutsCount(int newCount)
        {
            _outsCount = newCount;
        }
    }
}
