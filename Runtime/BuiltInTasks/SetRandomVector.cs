using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    public class SetRandomVector : BehaviourTask
    {
        #region Fields

        [SerializeField]
        [ValueType(typeof(Vector3Value))]
        private BlackboardName name;

        [SerializeField]
        private float radius = 1f;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Randomize {name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Blackboard.SetValue(name, (Vector3Value)(Random.insideUnitSphere * radius));
            yield return null;
        }
    }
}
