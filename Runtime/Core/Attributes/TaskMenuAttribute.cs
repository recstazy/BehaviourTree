using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary> Set task menu path in task selector dropdown </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TaskMenuAttribute : Attribute
    {
        #region Fields
	
        #endregion

        #region Properties
	
        public string Path { get; private set; }

        #endregion

        public TaskMenuAttribute(string menuPath)
        {
            Path = menuPath;
        }
    }
}
