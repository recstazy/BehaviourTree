using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Don't show this task in task selector popup on nodes in editor window
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExcludeFromTaskSelectorAttribute : Attribute { }
}
