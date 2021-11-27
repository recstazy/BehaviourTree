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

        private static readonly Color s_backColor = new Color(0f, 0.255f, 0.3f, 0.5f);

        #endregion

        #region Properties

        protected override Color Color => s_backColor;

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
