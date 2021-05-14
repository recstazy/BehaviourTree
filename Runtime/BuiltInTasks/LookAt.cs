using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [TaskOut(0)]
    public class LookAt : BehaviourTask
    {
        #region Fields

        [SerializeField]
        [ValueType(typeof(TransformValue))]
        private BlackboardName transform;

        [SerializeField]
        [ValueType(typeof(TransformValue), typeof(Vector3Value))]
        private BlackboardName target;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Look at {target}";
        }

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue<TransformValue>(transform, out var transformValue))
            {
                if (transformValue.Value != null)
                {
                    var target = GetTarget();

                    if (target != null)
                    {
                        transformValue.Value.LookAt(target, Vector3.up);
                    }
                    else
                    {
                        var pointTarget = GetTargetAsPosition();

                        if (pointTarget != null)
                        {
                            transformValue.Value.LookAt(pointTarget.Value, Vector3.up);
                        }
                    }
                }
            }

            yield return null;
        }

        private Transform GetTarget()
        {
            if (Blackboard.TryGetValue(target, out var iTarget))
            {
                if (iTarget is TransformValue transform)
                {
                    return transform;
                }
                else if (iTarget is GameObjectValue goValue)
                {
                    return goValue.Value.transform;
                }
            }

            return null;
        }

        private Vector3? GetTargetAsPosition()
        {
            if (Blackboard.TryGetValue<Vector3Value>(target, out var targetPosition))
            {
                return targetPosition.Value;
            }

            return null;
        }
    }
}
