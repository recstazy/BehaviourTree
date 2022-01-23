using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    public class InputGet<T> : InputBase
    {
        #region Fields

        private Func<T> _getMethod;

        #endregion

        #region Properties

        public override Type ValueType => typeof(T);
        public T Value => _getMethod == null ? default : _getMethod();

        #endregion

        [RuntimeInstanced]
        public void InitializeMethod(Func<T> getMethod)
        {
            _getMethod = getMethod;
        }
    }
}
