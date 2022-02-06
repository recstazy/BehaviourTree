using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    [TaskOut]
    [TaskMenu("Navigation/Stop NavAgent")]
    [NoInspector]
    public class StopNavAgent : NavAgentTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator NavAgentTaskRoutine(NavMeshAgent navAgent)
        {
            navAgent.ResetPath();
            yield return null;
        }
    }
}
