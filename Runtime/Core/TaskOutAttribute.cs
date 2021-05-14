using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Add out to this task type with index and name if needed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TaskOutAttribute : Attribute
    {
        public readonly string name;
        public readonly int index;

        /// <param name="index">Out index</param>
        /// <param name="name">Out name (optional)</param>
        public TaskOutAttribute(int index, string name = "")
        {
            this.name = name;
            this.index = index;
        }
    }
}
