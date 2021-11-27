using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Use on BlackboardName to constraint compatable types of blackboard property
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ValueTypeAttribute : PropertyAttribute
    {
        #region Fields

        #endregion

        #region Properties

        public Type[] CompatableTypes { get; private set; }

        #endregion

        public ValueTypeAttribute(params Type[] compatableTypes)
        {
            CompatableTypes = compatableTypes is null ? new Type[0] : compatableTypes;
        }
    }
}
