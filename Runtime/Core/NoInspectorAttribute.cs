using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Prevent showing Node Inspector in Behaviour Tree Window
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NoInspectorAttribute : Attribute { }
}
