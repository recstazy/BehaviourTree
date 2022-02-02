using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Add out to this task type and assign it an optional name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TaskOutAttribute : Attribute
    {
        public readonly string Name;

        /// <param name="name">Out name (optional)</param>
        public TaskOutAttribute(string name = "")
        {
            Name = name;
        }
    }
}
