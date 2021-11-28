using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Plays whole behaviour tree in PlayTreeRoutine
    /// </summary>
    public sealed class TreePlayer : IBlackboardProvider
    {
        #region Fields

        private BranchPlayer _treeBranchPlayer;
        private CoroutineRunner _coroutineRunner;
        private string _name;

        #endregion

        #region Properties

        /// <summary> Runtime tree instanced on Awake </summary>
        public BehaviourTree Tree { get; private set; }

        /// <summary> Actual tree asset </summary>
        public BehaviourTree SharedTree { get; private set; }

        public GameObject GameObject => _coroutineRunner.gameObject;
        public Blackboard Blackboard => Application.isPlaying ? Tree?.Blackboard : SharedTree?.Blackboard;
        public string FullName => $"{GameObject.name}{(string.IsNullOrEmpty(_name) ? string.Empty : $" ({_name})")}";

        internal static List<TreePlayer> PlayersCache { get; private set; } = new List<TreePlayer>();

        #endregion

        public TreePlayer(BehaviourTree tree, Blackboard blackboard, CoroutineRunner coroutineRunner, string name)
        {
            _name = name;
            _coroutineRunner = coroutineRunner;
            SharedTree = tree;
            Tree = tree.CreateRuntimeImplementation(coroutineRunner, blackboard);
            AddTreePlayer(this);
        }

        public IEnumerator PlayTreeRoutine()
        {
            _treeBranchPlayer = new BranchPlayer(Tree.EntryNode?.TaskImplementation);
            _treeBranchPlayer.Start();
            yield return new WaitUntil(() => !_treeBranchPlayer.IsRunning);
            yield return null;
        }

        internal void Destroyed()
        {
            RemoveTreePlayer(this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void ClearPlayersCache()
        {
            PlayersCache.Clear();
        }

        private static void AddTreePlayer(TreePlayer player)
        {
            if (player.SharedTree == null) return;
            PlayersCache.Add(player);
        }

        private static void RemoveTreePlayer(TreePlayer player)
        {
            if (player.SharedTree == null) return;
            PlayersCache.Remove(player);
        }
    }
}
