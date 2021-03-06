using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        internal TaskNodeData EntryNode => _nodeData?.TaskData == null || _nodeData.TaskData.Length == 0 ? null : _nodeData.TaskData[0];
        internal bool SnapEnabled { get => _snapEnabled; set => _snapEnabled = value; }

        #endregion

        internal void CreateEntry()
        {
            if (_nodeData != null && _nodeData.TaskData != null && _nodeData.TaskData.Length == 0)
            {
                _nodeData.TaskData = new TaskNodeData[] { new TaskNodeData(0, new EntryTask()) };
            }
        }

        [RuntimeInstanced]
        internal BehaviourTree CreateRuntimeImplementation(CoroutineRunner coroutineRunner, Blackboard blackboard)
        {
            var instance = Instantiate(this);
            instance.InitializeRuntime(coroutineRunner, blackboard);
            instance.InitializeNodesWithConnections();
            instance.IsRuntime = true;
            return instance;
        }

        [RuntimeInstanced]
        internal BehaviourTree CreateRuntimeImplementation(CoroutineRunner coroutineRunner)
        {
            if (_blackboard == null)
            {
                Debug.LogError($"No blackboard set for \"{name}\" tree");
            }

            return CreateRuntimeImplementation(coroutineRunner, _blackboard);
        }

        [RuntimeInstanced]
        private void InitializeRuntime(CoroutineRunner coroutineRunner, Blackboard blackboard)
        {
            _blackboard = Instantiate(blackboard);
            _blackboard.InitializeAtRuntime(coroutineRunner.gameObject);
            var runtimeNodeData = _nodeData.TaskData;

            for (int i = 0; i < runtimeNodeData.Length; i++)
            {
                if (runtimeNodeData[i].TaskImplementation != null)
                {
                    runtimeNodeData[i].TaskImplementation.Blackboard = _blackboard;
                    runtimeNodeData[i].TaskImplementation.SetCoroutineRunner(coroutineRunner);
                }
            }

            foreach (var d in _nodeData.FuncData)
            {
                if (d.FuncImplementation != null)
                {
                    d.FuncImplementation.SetBlackboard(_blackboard);
                }
            }
        }

        [RuntimeInstanced]
        private void InitializeNodesWithConnections()
        {
            foreach (var data in _nodeData)
            {
                data.InitialzeConnections(_nodeData);
            }
        }
    }
}
