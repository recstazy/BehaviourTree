using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)][TaskMenu("Tasks/SubTree")]
    public class SubTree : BehaviourTask
    {
        #region Fields

        [SerializeField]
        private BehaviourTree _tree;

        [SerializeField]
        private string _displayName = "Sub Tree";

        private TreePlayer _treePlayer;

        private static readonly Color s_backColor = new Color(0.1f, 0.2f, 0.3f, 0.5f);

        #endregion

        #region Properties

        protected override Color Color => s_backColor;
        public string DisplayName { get => _displayName; }

        #endregion

        public override string GetDescription()
        {
            return $"Play {(_tree == null ? "tree" : _tree.name)} as SubTree";
        }

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
