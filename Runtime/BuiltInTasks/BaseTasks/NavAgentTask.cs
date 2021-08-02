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

        [SerializeField]
        [ValueType(typeof(NavAgentValue))]
        protected BlackboardName _navAgent = CommonNames.NavAgent;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue<NavAgentValue>(_navAgent, out var navAgentValue))
            {
                if (navAgentValue != null && navAgentValue.Value != null)
                {
                    yield return NavAgentTaskRoutine(navAgentValue.Value);
                }
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
