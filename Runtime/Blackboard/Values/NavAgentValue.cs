using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    public class NavAgentValue : ObjectValue<NavMeshAgent>
    {
        public NavAgentValue() { }
        public NavAgentValue(NavMeshAgent value) : base(value) { }
    }
}
