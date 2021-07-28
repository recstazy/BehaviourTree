using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Navigation/Stop NavAgent")]
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
