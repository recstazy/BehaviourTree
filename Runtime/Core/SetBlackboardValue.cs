using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public class SetBlackboardValue : MonoBehaviour, IBlackboardProvider
    {
        #region Fields

        [SerializeField]
        private BehaviourPlayer _player;

        [SerializeField]
        private BlackboardValueMapping _newValue;

        [SerializeField]
        private bool _onStart;

        #endregion

        #region Properties

        public Blackboard Blackboard => _player?.Blackboard;

        #endregion

        private void Start()
        {
            if (_onStart)
            {
                Execute();
            }
        }

        public void Execute()
        {
            if (_player != null && Blackboard != null)
            {
                _player.Blackboard.SetValue(_newValue.BlackboardName, _newValue.Value);
            }
        }
    }
}
