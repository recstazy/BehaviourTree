using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    public class MoveNavAgent : NavAgentTask
    {
        #region Fields

        [ValueType(typeof(Vector3Value), typeof(TransformValue), typeof(GameObjectValue))]
        [SerializeField]
        private BlackboardName destination;

        [SerializeField]
        [Tooltip("Wait until agent reached destination")]
        private bool waitForFinished = false;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Move to {destination}";
        }

        protected override IEnumerator NavAgentTaskRoutine(NavMeshAgent navAgent)
        {
            if (Blackboard.TryGetValue(destination, out var destinationValue))
            {
                Vector3? destination = null;

                if (destinationValue is Vector3Value vectorValue)
                {
                    destination = vectorValue;
                }
                else if (destinationValue is TransformValue transformValue)
                {
                    if (transformValue?.Value != null)
                    {
                        destination = transformValue.Value.position;
                    }
                }
                else if (destinationValue is GameObjectValue goValue)
                {
                    if (goValue?.Value != null)
                    {
                        destination = goValue.Value.transform.position;
                    }
                }

                if (destination != null)
                {
                    navAgent.SetDestination(destination.Value);
                }
            }

            if (waitForFinished)
            {
                yield return new WaitUntil(() => IsReachedDestination(navAgent));
            }

            yield return null;
        }

        private bool IsReachedDestination(NavMeshAgent navAgent)
        {
            if (!navAgent.pathPending)
            {
                if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
