using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public interface IGameObjectProvider
    {
        GameObject gameObject { get; }
    }
}
