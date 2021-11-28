using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public sealed class TreePlayer
    {
        #region Fields

        [SerializeField]
        private BehaviourTree _tree;

        private BranchPlayer _treeBranchPlayer;

        #endregion

        #region Properties

        /// <summary> Runtime tree instanced on Awake </summary>
        public BehaviourTree Tree { get; private set; }

        /// <summary> Actual tree asset </summary>
        public BehaviourTree SharedTree => _tree;

        #endregion

        public TreePlayer(BehaviourTree tree, Blackboard blackboard, CoroutineRunner coroutineRunner)
        {
            _tree = tree;
            Tree = tree.CreateRuntimeImplementation(coroutineRunner, blackboard);
        }

        public IEnumerator PlayTreeRoutine()
        {
            _treeBranchPlayer = new BranchPlayer(Tree.EntryNode?.TaskImplementation);
            _treeBranchPlayer.Start();
            yield return new WaitUntil(() => !_treeBranchPlayer.IsRunning);
            yield return null;
        }
    }
}
