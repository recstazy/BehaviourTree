using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    public abstract class InputBase 
    {
        public abstract Type ValueType { get; }
    }
}
