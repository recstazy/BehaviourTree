using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Marks methods which made to be called only on instanced (not shared) objects
    /// </summary>
    internal class RuntimeInstancedAttribute : Attribute { }
}
