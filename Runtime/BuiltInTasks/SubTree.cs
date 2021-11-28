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

        private TreePlayer _treePlayer;

        private static readonly Color s_backColor = new Color(0.1f, 0.2f, 0.3f, 0.5f);

        #endregion

        #region Properties

        protected override Color Color => s_backColor;

        #endregion

        public override string GetDescription()
        {
            return $"Play {(_tree == null ? "tree" : _tree.name)} as SubTree";
        }

        protected override void Initialized()
        {
            _treePlayer = new TreePlayer(_tree, _tree.Blackboard, CoroutineRunner, $"SubTree");
        }

        protected override IEnumerator TaskRoutine()
        {
            yield return _treePlayer.PlayTreeRoutine();
            Succeed = _treePlayer.Succeed;
        }
    }
}
