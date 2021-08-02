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
        private BehaviourTree _tree;

        [SerializeField]
        private bool _playOnAwake = true;

        [Header("Set blackboard scene references here")]
        [SerializeField]
        private List<BlackboardValueMapping> _setOnStart;

        private BranchPlayer _treePlayer;
        private Coroutine _playRoutine;
        private CoroutineRunner _coroutineRunner;

        #endregion

        #region Properties
		
        /// <summary> Runtime tree instanced on Awake </summary>
        public BehaviourTree Tree { get; private set; }

        /// <summary> Actual tree asset </summary>
        public BehaviourTree SharedTree => _tree;

        public bool IsPlaying => _playRoutine != null;
        public BehaviourTask CurrentTask { get; private set; }
        public Blackboard Blackboard => Application.isPlaying ? Tree?.Blackboard : SharedTree?.Blackboard;

        #endregion

        private void Reset()
        {
            _playOnAwake = true;
        }

        private void Awake()
        {
            _coroutineRunner = gameObject.AddComponent<CoroutineRunner>();

            if (_playOnAwake)
            {
                Initialize(_tree);
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
            _tree = tree;
            if (_tree == null) return;

            Tree = _tree?.CreateRuntimeImplementation(_coroutineRunner);

            foreach (var value in _setOnStart)
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
                    _treePlayer = new BranchPlayer(Tree.EntryNode?.TaskImplementation);
                    _playRoutine = StartCoroutine(PlayRoutine());
                }
            }
            else
            {
                if (IsPlaying)
                {
                    StopCoroutine(_playRoutine);
                    _playRoutine = null;
                    _treePlayer = null;
                }
            }
        }

        private IEnumerator PlayRoutine()
        {
            while(true)
            {
                _treePlayer.Start();
                yield return new WaitUntil(() => !_treePlayer.IsRunning);
                yield return null;
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
    }
}
