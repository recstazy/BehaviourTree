using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Component used to play behaviour trees
    /// </summary>
    public sealed class BehaviourPlayer : MonoBehaviour, IBlackboardProvider
    {
        internal static event System.Action OnInstancedOrDestroyed;

        #region Fields

        [SerializeField]
        private BehaviourTree _tree;

        [SerializeField]
        private bool _playOnStart = true;

        private TreePlayer _treePlayer;
        private Coroutine _playRoutine;
        private CoroutineRunner _coroutineRunner;

        #endregion

        #region Properties

        /// <summary> Runtime tree instanced on Awake </summary>
        public BehaviourTree Tree => _treePlayer.Tree;

        /// <summary> Actual tree asset </summary>
        public BehaviourTree SharedTree => _tree;

        public bool IsPlaying => _playRoutine != null;
        public Blackboard Blackboard => Application.isPlaying ? Tree?.Blackboard : SharedTree?.Blackboard;

        #endregion

        private void Reset()
        {
            _playOnStart = true;
        }

        private void Start()
        {
            _coroutineRunner = gameObject.AddComponent<CoroutineRunner>();

            if (_playOnStart)
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
                var playersOnThisGameObject = TreePlayer.PlayersCache.Where(p => p.GameObject == gameObject).ToArray();

                foreach (var player in playersOnThisGameObject)
                {
                    player.Destroyed();
                }

                OnInstancedOrDestroyed?.Invoke();
            }
        }

        /// <summary> Set new behaviour tree asset in runtime </summary>
        public void Initialize(BehaviourTree tree)
        {
            Clear();
            _tree = tree;
            if (_tree == null) return;

            _treePlayer = new TreePlayer(_tree, _tree.Blackboard, _coroutineRunner, "Main");
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
                    _playRoutine = StartCoroutine(PlayRoutine());
                }
            }
            else
            {
                if (IsPlaying)
                {
                    StopCoroutine(_playRoutine);
                    _playRoutine = null;
                }
            }
        }

        private IEnumerator PlayRoutine()
        {
            while (true)
            {
                yield return _treePlayer.PlayTreeRoutine();
            }
        }

        private void Clear()
        {
            if (IsPlaying)
            {
                SetIsPlaying(false);
            }
        }
    }
}
