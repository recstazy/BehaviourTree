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
        private Vector3 left;

        [SerializeField]
        private Vector3 right;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Randomize {name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Vector3 newVector = new Vector3(
                Random.Range(left.x, right.x), 
                Random.Range(left.y, right.y), 
                Random.Range(left.z, right.z));

            Blackboard.SetValue(name, (Vector3Value)newVector);
            yield return null;
        }
    }
}
