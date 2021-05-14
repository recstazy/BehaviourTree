using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class CustomNodeDrawerAttribute : Attribute
    {
        #region Fields

        #endregion

        #region Properties

        public Type TaskType { get; private set; }

        #endregion

        public CustomNodeDrawerAttribute(Type taskType)
        {
            if (taskType.IsSubclassOf(typeof(BehaviourTask)))
            {
                TaskType = taskType;
            }
        }
    }
}
