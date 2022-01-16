using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    /// <summary> UIElements analog of CustomPropertyDrawer </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomPropertyElementAttribute : Attribute
    {
        #region Fields

        #endregion

        #region Properties
	
        public Type PropertyType { get; private set; }

        #endregion

        public CustomPropertyElementAttribute(Type propertyType)
        {
            PropertyType = propertyType;
        }
    }
}
