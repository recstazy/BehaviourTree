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
        private TaskStack _taskStack;

        #endregion

        #region Properties

        /// <summary> Runtime tree instanced on Awake </summary>
        public BehaviourTree Tree { get; private set; }

        /// <summary> Actual tree asset </summary>
        public BehaviourTree SharedTree { get; private set; }

        public GameObject GameObject => _coroutineRunner.gameObject;
        public Blackboard Blackboard => Application.isPlaying ? Tree?.Blackboard : SharedTree?.Blackboard;
        public string FullName => GetFullName();
        public bool Succeed => _treeBranchPlayer.BranchSucceed;

        internal static List<TreePlayer> PlayersCache { get; private set; } = new List<TreePlayer>();
        internal TaskStack Stack => _taskStack;

        #endregion

        public TreePlayer(BehaviourTree tree, Blackboard blackboard, CoroutineRunner coroutineRunner, string name)
        {
            _name = name;
            _coroutineRunner = coroutineRunner;
            SharedTree = tree;
            Tree = tree.CreateRuntimeImplementation(coroutineRunner, blackboard);
            
            if (Application.isEditor)
            {
                _taskStack = new TaskStack(tree.NodeData);
                BindToTreeTasks();
            }
           
            AddTreePlayer(this);
        }

        public IEnumerator PlayTreeRoutine()
        {
            _taskStack.Clear();
            _treeBranchPlayer = new BranchPlayer(Tree.EntryNode?.TaskImplementation, BehaviourTask.NoOut);
            _treeBranchPlayer.Start();
            yield return new WaitUntil(() => !_treeBranchPlayer.IsRunning);
            yield return null;
        }

        internal void Destroyed()
        {
            if (Application.isEditor) UnbindFromTreeTasks();
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

        private string GetFullName()
        {
            string treeName = SharedTree != null ? SharedTree.name : "[No Tree]";
            string customName = string.IsNullOrEmpty(_name) ? string.Empty : $" ({_name})";
            return $"{GameObject.name} - {treeName} {customName}";
        }

        private void BindToTreeTasks()
        {
            foreach (var t in Tree.NodeData.Data)
            {
                t.TaskImplementation.OnStarted += TaskStarted;
            }
        }

        private void UnbindFromTreeTasks()
        {
            foreach (var t in Tree.NodeData.Data)
            {
                t.TaskImplementation.OnStarted -= TaskStarted;
            }
        }

        private void TaskStarted(BehaviourTask task)
        {
            _taskStack.TaskStarted(task.Index, task.StartedFromOutPin);
        }
    }
}
