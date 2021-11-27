using System;

namespace Recstazy.BehaviourTree
{
    /// <summary> Hide this property from behaviour tree </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HideInTreeAttribute : Attribute { }
}
