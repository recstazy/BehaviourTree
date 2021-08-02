using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Value/Set Random Vector")]
    public class SetRandomVector : BehaviourTask
    {
        #region Fields

        [SerializeField]
        [ValueType(typeof(Vector3Value))]
        private BlackboardName _name;

        [SerializeField]
        private Vector3 _left;

        [SerializeField]
        private Vector3 _right;

        #endregion

        #region Properties

        #endregion

        public override string GetDescription()
        {
            return $"Randomize {_name}";
        }

        protected override IEnumerator TaskRoutine()
        {
            Vector3 newVector = new Vector3(
                Random.Range(_left.x, _right.x), 
                Random.Range(_left.y, _right.y), 
                Random.Range(_left.z, _right.z));

            Blackboard.SetValue(_name, (Vector3Value)newVector);
            yield return null;
        }
    }
}
