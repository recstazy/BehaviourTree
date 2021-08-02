using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Navigation/Move To")]
    public class MoveNavAgent : NavAgentTask
    {
        #region Fields

        [ValueType(typeof(Vector3Value), typeof(IGameObjectProvider))]
        [SerializeField]
        private BlackboardName _destination;

        [SerializeField]
        [Tooltip("Wait until agent reached destination")]
        private bool _waitForFinished = false;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Move to {_destination}";
        }

        protected override IEnumerator NavAgentTaskRoutine(NavMeshAgent navAgent)
        {
            if (Blackboard.TryGetValue(_destination, out var destinationValue))
            {
                Vector3? destination = null;

                if (destinationValue is Vector3Value vectorValue)
                {
                    destination = vectorValue;
                }
                else if (destinationValue is IGameObjectProvider goProvider)
                {
                    if (goProvider.gameObject != null)
                    {
                        destination = goProvider.gameObject.transform.position;
                    }
                }

                if (destination != null)
                {
                    navAgent.SetDestination(destination.Value);
                }
            }

            if (_waitForFinished)
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
