using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Implement to be able to use <c>BlackboardName</c> in custom script
    /// </summary>
    public interface IBlackboardProvider
    {
        Blackboard Blackboard { get; }
    }
}
