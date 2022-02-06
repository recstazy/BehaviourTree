using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base task to work with Nav Mesh Agent
    /// </summary>
    [System.Serializable]
    [ExcludeFromTaskSelector]
    public class NavAgentTask : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.NavAgent != null)
            {
                yield return NavAgentTaskRoutine(Blackboard.NavAgent);
            }

            yield return null;
        }

        /// <summary>
        /// NavMeshAgent work made here
        /// </summary>
        protected virtual IEnumerator NavAgentTaskRoutine(NavMeshAgent navAgent)
        {
            yield return null;
        }
    }
}
