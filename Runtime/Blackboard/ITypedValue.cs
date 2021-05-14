using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Return <c>TypeError</c> if you can't compare this type with some other. Use only reasonable options.
    /// </summary>
    public enum CompareResult { TypeError = 0, NotEqual = 1, Less = 2, Equal = 3, More = 4 }

    /// <summary>
    /// Implement to make your own type appear in blackboard
    /// </summary>
    public interface ITypedValue
    {
        /// <summary> Provide the logic of how to compare your type with this and other blackboard types. </summary>
        /// <param name="other">Value which we're comparing with</param>
        CompareResult Compare(ITypedValue other);

        /// <summary> The value on witch this container depends </summary>
        object MainValue { get; }
    }
}
