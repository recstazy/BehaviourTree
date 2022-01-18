using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut][TaskMenu("Tasks/SubTree")]
    public class SubTree : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private BehaviourTree _tree;

        [SerializeField]
        private string _displayName = "Sub Tree";

        private TreePlayer _treePlayer;

        #endregion

        #region Properties

        public string DisplayName { get => _displayName; }

        #endregion

        protected override void Initialized()
        {
            _treePlayer = new TreePlayer(_tree, _tree.Blackboard, CoroutineRunner, _displayName);
        }

        protected override IEnumerator TaskRoutine()
        {
            yield return _treePlayer.PlayTreeRoutine();
            Succeed = _treePlayer.Succeed;
        }
    }
}
