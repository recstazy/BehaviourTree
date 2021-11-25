using System;

namespace Recstazy.BehaviourTree
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HideInWatcherAttribute : Attribute { }
}
