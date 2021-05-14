using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public class SetBlackboardValue : MonoBehaviour, IBlackboardProvider
    {
        #region Fields

        [SerializeField]
        private BehaviourPlayer player;

        [SerializeField]
        private BlackboardValueMapping newValue;

        [SerializeField]
        private bool onStart;

        #endregion

        #region Properties

        public Blackboard Blackboard => player?.Blackboard;

        #endregion

        private void Start()
        {
            if (onStart)
            {
                Execute();
            }
        }

        public void Execute()
        {
            player?.Blackboard?.SetValue(newValue.BlackboardName, newValue.Value);
        }
    }
}
