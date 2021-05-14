using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Component used to play behaviour trees
    /// </summary>
    public sealed class BehaviourPlayer : MonoBehaviour, IBlackboardProvider
    {
        internal static event Action OnInstancedOrDestroyed;

        #region Fields

        [SerializeField]
        private BehaviourTree tree;

        [SerializeField]
        private bool playOnAwake;

        [Header("Set blackboard scene references here")]
        [SerializeField]
        private List<BlackboardValueMapping> setOnStart;

        private BranchPlayer treePlayer;
        private Coroutine playRoutine;
        private CoroutineRunner coroutineRunner;

        #endregion

        #region Properties
		
        /// <summary> Runtime tree instanced on Awake </summary>
        public BehaviourTree Tree { get; private set; }

        /// <summary> Actual tree asset </summary>
        public BehaviourTree SharedTree => tree;

        public bool IsPlaying => playRoutine != null;
        public BehaviourTask CurrentTask { get; private set; }
        public Blackboard Blackboard => Application.isPlaying ? Tree?.Blackboard : SharedTree?.Blackboard;

        #endregion

        private void Reset()
        {
            playOnAwake = true;
        }

        private void Awake()
        {
            coroutineRunner = gameObject.AddComponent<CoroutineRunner>();

            if (playOnAwake)
            {
                Initialize(tree);
                Play();
            }

            if (Application.isEditor)
            {
                OnInstancedOrDestroyed?.Invoke();
            }
        }

        private void OnDestroy()
        {
            Clear();

            if (Application.isEditor)
            {
                OnInstancedOrDestroyed?.Invoke();
            }
        }

        // Dummy to show enabled checkmark
        private void OnEnable() { }

        /// <summary> Set new behaviour tree asset in runtime </summary>
        public void Initialize(BehaviourTree tree)
        {
            Clear();
            this.tree = tree;
            Tree = this.tree?.CreateRuntimeImplementation(coroutineRunner);
            if (Tree is null) return;

            foreach (var value in setOnStart)
            {
                Tree.Blackboard.SetValue(value.BlackboardName, value.Value);
            }
        }

        /// <summary> Play tree from start </summary>
        public void Play()
        {
            SetIsPlaying(true);
        }

        /// <summary> Stop playing tree </summary>
        public void Stop()
        {
            SetIsPlaying(false);
        }

        /// <summary> Same as Play/Stop but through boolean parameter </summary>
        public void SetIsPlaying(bool isPlaying)
        {
            if (isPlaying)
            {
                if (!IsPlaying && Tree != null)
                {
                    treePlayer = new BranchPlayer(Tree.EntryNode?.TaskImplementation);
                    playRoutine = StartCoroutine(PlayRoutine());
                }
            }
            else
            {
                if (IsPlaying)
                {
                    StopCoroutine(playRoutine);
                    playRoutine = null;
                    treePlayer = null;
                }
            }
        }

        internal void FetchCommonValues()
        {
            var mapping = new BlackboardValueMapping(CommonNames.GameObject, new GameObjectValue(gameObject));
            AddOrSetToSetOnStart(mapping);
            mapping = new BlackboardValueMapping(CommonNames.Transform, new TransformValue(transform));
            AddOrSetToSetOnStart(mapping);
            mapping = new BlackboardValueMapping(CommonNames.Rigidbody, new RigidbodyValue(GetComponentInChildren<Rigidbody>(true)));
            AddOrSetToSetOnStart(mapping, true);
            mapping = new BlackboardValueMapping(CommonNames.NavAgent, new NavAgentValue(GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(true)));
            AddOrSetToSetOnStart(mapping);
        }

        private IEnumerator PlayRoutine()
        {
            while(true)
            {
                yield return treePlayer.PlayBranchRoutine();
            }
        }

        private void Clear()
        {
            if (IsPlaying)
            {
                SetIsPlaying(false);
            }

            if (Tree != null)
            {
                Destroy(Tree);
                Tree = null;
            }
        }

        private int SetOnStartIndexByName(string name)
        {
            for (int i = 0; i < setOnStart.Count; i++)
            {
                if (setOnStart[i] != null && setOnStart[i].BlackboardName == name)
                {
                    return i;
                }
            }

            return -1;
        }

        private void AddOrSetToSetOnStart(BlackboardValueMapping mapping, bool removeNull = false)
        {
            int index = SetOnStartIndexByName(mapping.BlackboardName);

            if (mapping.Value.Compare(null) == CompareResult.Equal)
            {
                if (removeNull)
                {
                    if (index >= 0)
                    {
                        setOnStart.RemoveAt(index);
                    }
                }

                return;
            }

            if (index >= 0)
            {
                setOnStart[index] = mapping;
            }
            else
            {
                bool blackboardContainsName = Array.IndexOf(SharedTree.Blackboard.GetNames(), mapping.BlackboardName) >= 0;

                if (!blackboardContainsName)
                {
                    SharedTree.Blackboard.AddNameInEditor(mapping.BlackboardName);
                }
                
                setOnStart.Add(mapping);
            }
        }
    }
}
