using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Asset containing all the behaviour tree logic
    /// </summary>
    [CreateAssetMenu(menuName = "Behaviour Tree/New Behaviour Tree", fileName = "NewBehaviourTree", order = 131)]
    public sealed class BehaviourTree : ScriptableObject, IBlackboardProvider
    {
        #region Fields

        [SerializeField]
        private Blackboard _blackboard;

        [SerializeField]
        private Vector2 _graphPosition;

        [SerializeField]
        private float _zoom = 1f;

        [SerializeField]
        private bool _snapEnabled = true;

        [SerializeField]
        private TreeNodeData _nodeData;

        #endregion

        #region Properties

        /// <summary> Is this tree a runtime instance </summary>
        public bool IsRuntime { get; private set; } = false;
        public Blackboard Blackboard => _blackboard;

        internal TreeNodeData NodeData { get => _nodeData; set => _nodeData = value; }
        internal Vector2 GraphPosition { get => _graphPosition; set => _graphPosition = value; }
        internal float Zoom { get => _zoom; set => _zoom = value; }
        internal NodeData EntryNode => _nodeData?.Data == null || _nodeData.Data.Length == 0 ? null : _nodeData.Data[0];
        internal bool SnapEnabled { get => _snapEnabled; set => _snapEnabled = value; }

        #endregion

        internal void CreateEntry()
        {
            if (_nodeData != null && _nodeData.Data != null && _nodeData.Data.Length == 0)
            {
                _nodeData.Data = new NodeData[] { new NodeData(0, new EntryTask()) };
            }
        }

        [RuntimeInstanced]
        internal BehaviourTree CreateRuntimeImplementation(CoroutineRunner coroutineRunner)
        {
            if (Blackboard is null)
            {
                Debug.LogError($"No blackboard set for \"{name}\" tree");
            }

            var instance = Instantiate(this);
            instance.InitializeRuntime(coroutineRunner);
            instance.CreateRuntimeConnections();
            instance.IsRuntime = true;
            return instance;
        }

        [RuntimeInstanced]
        private void InitializeRuntime(CoroutineRunner coroutineRunner)
        {
            _blackboard = Instantiate(_blackboard);
            _blackboard.InitializeAtRuntime(coroutineRunner.gameObject);
            var runtimeNodeData = _nodeData.Data;

            for (int i = 0; i < runtimeNodeData.Length; i++)
            {
                if (runtimeNodeData[i].TaskImplementation != null)
                {
                    runtimeNodeData[i].TaskImplementation.Blackboard = Blackboard;
                    runtimeNodeData[i].TaskImplementation.SetCoroutineRunner(coroutineRunner);
                }
            }
        }

        [RuntimeInstanced]
        private void CreateRuntimeConnections()
        {
            foreach (var d in _nodeData.Data)
            {
                d.CreateRuntimeConnections(_nodeData.Data);
            }
        }
    }
}
